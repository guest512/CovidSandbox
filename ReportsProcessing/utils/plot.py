from datetime import datetime as _dt
from matplotlib import figure as _figure
import pandas as _pd
from . import dates as _dates

_dateFormat = '%d-%m-%Y'


def key_russian_dates(ax: _figure.Axes):
    ''' Draws key dates from Russia on plot as vertical lines and spans. '''

    ax.axvline(_dt.strptime('01-04-2020', _dateFormat),
               label='Путин: Начало нерабочих дней', color='Red')
    ax.axvline(_dt.strptime('12-05-2020', _dateFormat),
               label='Путин: Пик пройден', color='Cyan')
    ax.axvline(_dt.strptime('15-06-2020', _dateFormat),
               label='Путин: Выход из эпидемии', color='Green')
    ax.axvline(_dt.strptime('24-06-2020', _dateFormat), label='Парад')
    ax.axvspan(_dt.strptime('25-06-2020', _dateFormat),
               _dt.strptime('01-07-2020', _dateFormat), label='Конституция', alpha=0.2, color='Green')


def _setup_axes_for_russian_regions_stat(ax: _figure.Axes, title: str = None):
    ax.xaxis_date()
    key_russian_dates(ax)

    ax.set_ylim(bottom=0)
    ax.legend(loc='upper left')
    ax.grid(axis='y', color='black', linestyle='dashed', alpha=0.4)

    if title:
        ax.set_title(title)


def bar_with_sma_line(ax: _figure.Axes, values: _pd.Series, sma_window: int = 7, label: str = None, bar_alpha: float = 0.3):
    if label:
        ax.plot(values.rolling(window=sma_window).mean(),
                label=label+"-SMA"+str(sma_window))
    else:
        ax.plot(values.rolling(window=sma_window).mean())

    ax.bar(values.index, values, alpha=bar_alpha)


def report(fig: _figure.Figure, df: _pd.DataFrame, name: str = None):
    dfdf = df
    dfdf_weekly = dfdf.resample("1W").sum()
    dfdf_monthly = dfdf.resample("1M").sum()

    index = dfdf.index
    index_weekly = dfdf_weekly.index
    index_monthly = dfdf_monthly.index

    confirmed_daily = dfdf.Confirmed_Change
    recovered_daily = dfdf.Recovered_Change
    deaths_daily = dfdf.Deaths_Change

    confirmed_weekly = dfdf_weekly.Confirmed_Change
    confirmed_monthly = dfdf_monthly.Confirmed_Change

    recovered_weekly = dfdf_weekly.Recovered_Change
    recovered_monthly = dfdf_monthly.Recovered_Change

    deaths_weekly = dfdf_weekly.Deaths_Change
    deaths_monthly = dfdf_monthly.Deaths_Change

    active = dfdf.Active

    ax = fig.add_subplot(2, 2, 1)

    bar_with_sma_line(ax, confirmed_daily, label="Заболевшие")
    bar_with_sma_line(ax, recovered_daily, label="Выздоровевшие")

    ax.bar(index, deaths_daily, label='Смерти',
           alpha=0.3, bottom=recovered_daily)
    ax.plot(index, (recovered_daily +
                    deaths_daily).rolling(window=7).mean(), label='Смерти-SMA7')

    _setup_axes_for_russian_regions_stat(ax, "Статистика день ко дню")

    ax = fig.add_subplot(2, 2, 2)

    bar_with_sma_line(ax, active, label="Больные")
    _setup_axes_for_russian_regions_stat(ax, "Количество больных")

    ax = fig.add_subplot(2, 2, 3)

    ax.bar(index_weekly, confirmed_weekly, label='Заболевшие', width=2)
    ax.bar(index_weekly + _dates.one_day, recovered_weekly,
           label='Выздоровевшие', width=2)
    ax.bar(index_weekly + _dates.one_day * 2,
           deaths_weekly, label='Смерти', width=2)

    _setup_axes_for_russian_regions_stat(ax, "Статистика неделя к неделе")

    ax = fig.add_subplot(2, 2, 4)

    ax.bar(index_monthly, confirmed_monthly, label='Заболевшие', width=2)
    ax.bar(index_monthly + _dates.one_day,
           recovered_monthly, label='Выздоровевшие', width=2)
    ax.bar(index_monthly + _dates.one_day * 2,
           deaths_monthly, label='Смерти', width=2)

    _setup_axes_for_russian_regions_stat(ax, "Статистика месяц к месяцу")

    if name:
        fig.suptitle(name)
