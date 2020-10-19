from datetime import datetime as _dt
from matplotlib import figure as _figure, colors as _colors
import pandas as _pd
import numpy as _np
from . import dates as _dates

_dateFormat = '%d-%m-%Y'


def key_russian_dates(ax: _figure.Axes):
    ''' Draws key dates from Russia on plot as vertical lines and spans. '''
    ax.axvline(_dt.strptime('01-04-2020', _dateFormat),
               label='Путин: Начало нерабочих дней',
               color='Red')
    ax.axvline(_dt.strptime('12-05-2020', _dateFormat),
               label='Путин: Пик пройден',
               color='Cyan')
    ax.axvline(_dt.strptime('15-06-2020', _dateFormat),
               label='Путин: Выход из эпидемии',
               color='Green')
    ax.axvline(_dt.strptime('24-06-2020', _dateFormat), label='Парад')
    ax.axvspan(_dt.strptime('25-06-2020', _dateFormat),
               _dt.strptime('01-07-2020', _dateFormat),
               label='Конституция',
               alpha=0.2,
               color='Green')


def _setup_axes_for_russian_regions_stat(ax: _figure.Axes,
                                         title: str = None,
                                         grid: bool = True):
    ax.xaxis_date()
    key_russian_dates(ax)

    ax.set_ylim(bottom=0)
    ax.legend(loc='upper left')
    if grid:
        ax.grid(axis='y', color='black', linestyle='dashed', alpha=0.4)

    if title:
        ax.set_title(title)


def bar_with_sma_line(ax: _figure.Axes,
                      values: _pd.Series,
                      sma_window: int = 7,
                      label: str = None,
                      bar_alpha: float = 0.3):
    if label:
        ax.plot(values.rolling(window=sma_window).mean(),
                label=label + "-SMA" + str(sma_window))
    else:
        ax.plot(values.rolling(window=sma_window).mean())

    ax.bar(values.index, values, alpha=bar_alpha)


def _draw_daily_stats(ax: _figure.Axes, df: _pd.DataFrame):
    index = df.index
    confirmed_daily = df.Confirmed_Change
    recovered_daily = df.Recovered_Change
    deaths_daily = df.Deaths_Change

    bar_with_sma_line(ax, confirmed_daily, label="Заболевшие")
    bar_with_sma_line(ax, recovered_daily, label="Выздоровевшие")

    ax.bar(index,
           deaths_daily,
           label='Смерти',
           alpha=0.3,
           bottom=recovered_daily)
    ax.plot(index, (recovered_daily + deaths_daily).rolling(window=7).mean(),
            label='Смерти-SMA7')

    _setup_axes_for_russian_regions_stat(ax, "Статистика день ко дню")


def _draw_active_stats(ax: _figure.Axes, df: _pd.DataFrame):
    active = df.Active

    bar_with_sma_line(ax, active, label="Больные")
    _setup_axes_for_russian_regions_stat(ax, "Количество больных")


def _draw_weekly_stats(ax: _figure.Axes, df: _pd.DataFrame):
    _draw_stats_bar(ax, df.resample("1W").sum())
    _setup_axes_for_russian_regions_stat(ax, "Статистика неделя к неделе")


def _draw_monthly_stats(ax: _figure.Axes, df: _pd.DataFrame):
    _draw_stats_bar(ax, df.resample("1M").sum())
    _setup_axes_for_russian_regions_stat(ax, "Статистика месяц к месяцу")


def _draw_stats_bar(ax: _figure.Axes, df: _pd.DataFrame):
    index = df.index
    confirmed = df.Confirmed_Change
    recovered = df.Recovered_Change
    deaths = df.Deaths_Change

    ax.bar(index, confirmed, label='Заболевшие', width=2)
    ax.bar(index + _dates.one_day, recovered, label='Выздоровевшие', width=2)
    ax.bar(index + _dates.one_day * 2, deaths, label='Смерти', width=2)


def _draw_rt(ax, df):
    rt = df.Rt

    cmap = _colors.LinearSegmentedColormap.from_list('test',
                                                     [(0, 'darkGreen'),
                                                      (0.2, 'forestgreen'),
                                                      (0.4, 'yellowgreen'),
                                                      (0.5, 'gold'),
                                                      (0.7, 'orange'),
                                                      (0.9, 'orangered'),
                                                      (1, 'crimson')])
    norm = _colors.TwoSlopeNorm(1, 0.6, 1.4)
    colors = cmap(norm(rt))
    line_color = _np.array(cmap(norm(df.Rt[-7:].mean())))

    ax.bar(rt.index, rt.values, color=colors, alpha=0.3)
    ax.plot(rt.index, rt.rolling(window=3).mean(), color=line_color)

    _setup_axes_for_russian_regions_stat(
        ax, r"Коэффициент распространения ($R_t$)", False)
    ax.set_ylim(0.6, 2)
    ax.axhline(1, color='Red', linestyle='dashed', alpha=0.6)


def _draw_ttr(ax: _figure.Axes, df: _pd.DataFrame):
    ttr = df.Time_To_Resolve
    bar_with_sma_line(ax, ttr)
    _setup_axes_for_russian_regions_stat(ax, "Дней до исхода заражения")


def report(fig: _figure.Figure,
           df: _pd.DataFrame,
           name: str = None,
           start_date: _pd.Timestamp = None):

    report_funcs = [
        _draw_daily_stats, _draw_active_stats, _draw_weekly_stats,
        _draw_monthly_stats, _draw_rt, _draw_ttr
    ]

    for i in range(1, 7):
        ax = fig.add_subplot(3, 2, i)
        report_funcs[i - 1](ax, df)

        if start_date:
            ax.set_xlim(start_date)

    if name:
        fig.suptitle(name)
