import pandas as _pd

import contextlib as _ctxlib
from . import _dates, _path, _storage, _plot, _data

_dates_service = _dates._Dates(_path.__PathHelper())

(first_day, last_day) = _dates_service.get_available_dates()
first_week = first_day - _dates._Dates.to_Timedelta(first_day.weekday())
last_week = last_day - _dates._Dates.to_Timedelta(last_day.weekday())
one_day = _dates_service.to_Timedelta(1)
one_week = _dates_service.to_Timedelta(7)

plot = _plot._PlotHelper(_dates_service)
storage = _storage._Storage(_path.__PathHelper())
data = _data._DataHelper(storage)


def str_to_datetime(val: str) -> _pd.Timestamp:
    '''
    Converts string in the following format 'dd-mm-yyyy' to the pandas Timestamp object.

    Args:
        val(str): represents a date in string format.

    Returns:
        Converted to pandas Timestamp date.
    '''

    return _dates_service.to_Timestamp(val)


def days_to_timedelta(days: int) -> _pd.Timedelta:
    '''
    Converts a number of days to the pandas Timedelta object.

    Args:
        days(int): represents a number of days to convert.

    Returns:
        Converted to pandas Timedelta number of days.
    '''

    return _dates_service.to_Timedelta(days)


@_ctxlib.contextmanager
def setlocale_ctx(locale: str):
    '''
    Creates context with specifical locale. Could be useful to convert objects to string
    using regional settings (dates, numbers, etc.).

    Args:
        locale(str): represents a locale to set.

    Returns:
        Context with specified locale.
    '''

    import locale as loc

    saved = loc.setlocale(loc.LC_ALL)
    yield loc.setlocale(loc.LC_ALL, locale)
    loc.setlocale(loc.LC_ALL, saved)


del _dates, _path, _storage, _plot, _pd, _ctxlib, _data