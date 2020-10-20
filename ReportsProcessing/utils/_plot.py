from matplotlib import figure as _figure, colors as _colors
import pandas as _pd
import numpy as _np


class _PlotHelper():
    def __init__(self, dates):
        self.__dates = dates

    def key_russian_dates(self, ax: _figure.Axes):
        pass
        ''' Draws key dates from Russia on plot as vertical lines and spans. '''

        for (date, length, name) in self.__dates.get_key_russian_dates():
            x = None
            if length == self.__dates.to_Timedelta(1):
                ax.axvline(date, color='Gray', alpha=0.6)
                x = date
            else:
                ax.axvspan(date, date + length, color='Gray', alpha=0.6)
                x = date + length / 2

            ax.text(s=name,
                    x=x,
                    y=ax.get_ylim()[1] * .95,
                    rotation=90,
                    ha='center',
                    va='top',
                    fontsize='small',
                    bbox=dict(boxstyle="square,pad=0.6",
                              fc="white",
                              ec=(0.7, 0.7, 0.7),
                              lw=2))

    def _setup_axes_for_russian_regions_stat(self,
                                             ax: _figure.Axes,
                                             title: str = None,
                                             grid: bool = True,
                                             legend: bool = True,
                                             draw_key_dates: bool = True):
        ax.xaxis_date()

        if draw_key_dates:
            self.key_russian_dates(ax)

        if legend:
            ax.legend(loc='upper left')

        ax.set_ylim(bottom=0)

        if grid:
            ax.grid(axis='y', color='black', linestyle='dashed', alpha=0.4)

        if title:
            ax.set_title(title)

    def bar_with_sma_line(self,
                          ax: _figure.Axes,
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

    def _draw_daily_stats(self, ax: _figure.Axes, df: _pd.DataFrame,
                          draw_key_dates: bool):
        index = df.index
        confirmed_daily = df.Confirmed_Change
        recovered_daily = df.Recovered_Change
        deaths_daily = df.Deaths_Change

        self.bar_with_sma_line(ax, confirmed_daily, label="Заболевшие")
        self.bar_with_sma_line(ax, recovered_daily, label="Выздоровевшие")

        ax.bar(index,
               deaths_daily,
               label='Смерти',
               alpha=0.3,
               bottom=recovered_daily)
        ax.plot(index,
                (recovered_daily + deaths_daily).rolling(window=7).mean(),
                label='Смерти-SMA7')

        self._setup_axes_for_russian_regions_stat(
            ax, "Статистика день ко дню", draw_key_dates=draw_key_dates)

    def _draw_active_stats(self, ax: _figure.Axes, df: _pd.DataFrame,
                           draw_key_dates: bool):
        active = df.Active

        self.bar_with_sma_line(ax, active, label="Больные")
        self._setup_axes_for_russian_regions_stat(
            ax,
            "Количество больных",
            legend=False,
            draw_key_dates=draw_key_dates)

    def _draw_weekly_stats(self, ax: _figure.Axes, df: _pd.DataFrame,
                           draw_key_dates: bool):
        self._draw_stats_bar(ax, df.resample("1W").sum())
        self._setup_axes_for_russian_regions_stat(
            ax, "Статистика неделя к неделе", draw_key_dates=draw_key_dates)

    def _draw_monthly_stats(self, ax: _figure.Axes, df: _pd.DataFrame,
                            draw_key_dates: bool):
        self._draw_stats_bar(ax, df.resample("1M").sum())
        self._setup_axes_for_russian_regions_stat(
            ax, "Статистика месяц к месяцу", draw_key_dates=draw_key_dates)

    def _draw_stats_bar(self, ax: _figure.Axes, df: _pd.DataFrame):
        index = df.index
        confirmed = df.Confirmed_Change
        recovered = df.Recovered_Change
        deaths = df.Deaths_Change
        one_day = self.__dates.to_Timedelta(1)

        ax.bar(index, confirmed, label='Заболевшие', width=2)
        ax.bar(index + one_day, recovered, label='Выздоровевшие', width=2)
        ax.bar(index + one_day * 2, deaths, label='Смерти', width=2)

    def _draw_rt(self, ax, df, draw_key_dates):
        rt = df.Rt

        cmap = _colors.LinearSegmentedColormap.from_list(
            'test', [(0, 'darkGreen'), (0.2, 'forestgreen'),
                     (0.4, 'yellowgreen'), (0.5, 'gold'), (0.7, 'orange'),
                     (0.9, 'orangered'), (1, 'crimson')])
        norm = _colors.TwoSlopeNorm(1, 0.6, 1.4)
        colors = cmap(norm(rt))
        line_color = _np.array(cmap(norm(df.Rt[-7:].mean())))

        ax.bar(rt.index, rt.values, color=colors, alpha=0.3)
        ax.plot(rt.index, rt.rolling(window=3).mean(), color=line_color)

        ax.set_ylim(top=2)

        self._setup_axes_for_russian_regions_stat(
            ax, r"Коэффициент распространения ($R_t$)", False, False,
            draw_key_dates)

        ax.set_ylim(bottom=0.6)
        ax.axhline(1, color='Red', linestyle='dashed', alpha=0.6)

    def _draw_ttr(self, ax: _figure.Axes, df: _pd.DataFrame,
                  draw_key_dates: bool):
        ttr = df.Time_To_Resolve
        self.bar_with_sma_line(ax, ttr)
        self._setup_axes_for_russian_regions_stat(
            ax,
            "Дней до исхода заражения",
            legend=False,
            draw_key_dates=draw_key_dates)

    def report(self,
               fig: _figure.Figure,
               df: _pd.DataFrame,
               name: str = None,
               start_date: _pd.Timestamp = None,
               draw_key_dates: bool = False):

        report_funcs = [
            self._draw_daily_stats, self._draw_active_stats,
            self._draw_weekly_stats, self._draw_monthly_stats, self._draw_rt,
            self._draw_ttr
        ]

        for i in range(1, 7):
            ax = fig.add_subplot(3, 2, i)
            report_funcs[i - 1](ax, df, draw_key_dates)

            if start_date:
                ax.set_xlim(start_date)

        if name:
            fig.suptitle(name)
