import typing as _types
import os as _os
import pandas as _pd

from datetime import datetime as _datetime

# def to_Timestamp(val: _types.Union[str, _datetime]) -> _pd.Timestamp:
#     pass

# def to_datetime(val: _types.Union[str, _pd.Timestamp]) -> _datetime:
#     pass


def _get_dates() -> _types.Tuple[_pd.Timestamp, _pd.Timestamp]:

    reports_path = _os.path.abspath(
        _os.path.join(_os.path.dirname(__file__), "..", "data/reports/dayByDay"))
    files = _os.listdir(reports_path)
    start_date = _pd.Timestamp(files[0][:-4]).normalize()
    finish_date = _pd.Timestamp(files[-1][:-4]).normalize()

    if (len(_pd.read_csv(_os.path.join(reports_path, files[-1]))) == 1):
        finish_date = finish_date - _pd.Timedelta('1 days')

    return (start_date, finish_date)


first_day, last_day = _get_dates()
one_day = _pd.Timedelta('1 days')
