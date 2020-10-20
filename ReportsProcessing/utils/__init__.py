import pandas as _pd

import contextlib as _ctxlib
from . import _dates, _path, _storage, _plot

storage = _storage._Storage(_path.__PathHelper())

_dates_service = _dates._Dates(_path.__PathHelper())

(first_day, last_day) = _dates_service.get_available_dates()
one_day = _dates_service.to_Timedelta(1)
one_week = _dates_service.to_Timedelta(7)

plot = _plot._PlotHelper(_dates_service)


def str_to_datetime(val: str):
    return _dates_service.to_Timestamp(val)


def days_to_timedelta(days: int):
    return _dates_service.to_Timedelta(days)


def normalize(values: _pd.Series) -> _pd.Series:
    ''' Maps series of values to scale from 0.0 to 1.0 and returns it as a new Series object '''
    min = values.min()
    max = values.max()

    return (values - min) / (max - min)


@_ctxlib.contextmanager
def setlocale_ctx(locale: str):
    import locale as loc

    saved = loc.setlocale(loc.LC_ALL)
    yield loc.setlocale(loc.LC_ALL, locale)
    loc.setlocale(loc.LC_ALL, saved)


del _dates, _path, _storage, _plot, _pd, _ctxlib