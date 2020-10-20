import typing as _types
import os as _os
import pandas as _pd

from datetime import datetime as _datetime


class _Dates():
    def __init__(self, paths):
        self.__paths = paths

    def get_available_dates(
            self) -> _types.Tuple[_pd.Timestamp, _pd.Timestamp]:

        files = _os.listdir(self.__paths.get_daily_reports_path())
        start_date = _pd.Timestamp(files[0][:-4]).normalize()
        finish_date = _pd.Timestamp(files[-1][:-4]).normalize()

        if (len(
                _pd.read_csv(
                    _os.path.join(self.__paths.get_daily_reports_path(),
                                  files[-1]))) == 1):
            finish_date = finish_date - _pd.Timedelta('1 days')

        return (start_date, finish_date)

    def get_key_russian_dates(
            self
    ) -> _types.List[_types.Tuple[_pd.Timestamp, _pd.Timedelta, str]]:
        return list([
            (self.to_Timestamp('01-04-2020'), self.to_Timedelta(1),
             'Путин: Начало нерабочих дней'),
            (self.to_Timestamp('12-05-2020'), self.to_Timedelta(1),
             'Путин: Пик пройден'),
            (self.to_Timestamp('15-06-2020'), self.to_Timedelta(1),
             'Путин: Выход из эпидемии'),
            (self.to_Timestamp('24-06-2020'), self.to_Timedelta(1), 'Парад'),
            (self.to_Timestamp('25-06-2020'), self.to_Timedelta(7),
             'Конституция'),
        ])

    @staticmethod
    def to_Timestamp(val: str) -> _pd.Timestamp:
        return _pd.to_datetime(val, dayfirst=True)

    @staticmethod
    def to_Timedelta(days: int) -> _pd.Timedelta:
        return _pd.to_timedelta(str(days) + 'D')
