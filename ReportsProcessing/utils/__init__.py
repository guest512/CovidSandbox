import pandas as _pd

import contextlib as _ctxlib
from . import _dates, _path, _storage, _plot, _data

storage = _storage._Storage(_path.__PathHelper())

_dates_service = _dates._Dates(_path.__PathHelper())
data = _data._DataHelper(storage)

(first_day, last_day) = _dates_service.get_available_dates()
first_week = first_day - _dates._Dates.to_Timedelta(first_day.weekday())
last_week = last_day - _dates._Dates.to_Timedelta(last_day.weekday())
one_day = _dates_service.to_Timedelta(1)
one_week = _dates_service.to_Timedelta(7)

plot = _plot._PlotHelper(_dates_service)


def str_to_datetime(val: str):
    return _dates_service.to_Timestamp(val)


def days_to_timedelta(days: int):
    return _dates_service.to_Timedelta(days)


@_ctxlib.contextmanager
def setlocale_ctx(locale: str):
    import locale as loc

    saved = loc.setlocale(loc.LC_ALL)
    yield loc.setlocale(loc.LC_ALL, locale)
    loc.setlocale(loc.LC_ALL, saved)


del _dates, _path, _storage, _plot, _pd, _ctxlib, _data