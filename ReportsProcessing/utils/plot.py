from datetime import datetime as _dt
from matplotlib import axes as _axes

_dateFormat = '%d-%m-%Y'


def draw_key_russian_dates(ax: _axes.Axes):
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


def setup_axes_for_russian_regions_stat(ax: _axes.Axes, title: str):
    ax.xaxis_date()
    draw_key_russian_dates(ax)

    ax.set_ylim(bottom=0)
    ax.legend(loc='upper left')
    ax.grid(axis='y', color='black', linestyle='dashed', alpha=0.4)
    ax.set_title(title)