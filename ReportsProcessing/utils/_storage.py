import os as _os
import pandas as _pd
from typing import List as _List, Tuple as _Tuple


class _Storage():
    """
    docstring
    """
    def __init__(self, path):
        """
        docstring
        """
        self.__paths = path
        pass

    def get_country_regions(self, country_name: str) -> _List[str]:
        '''Returns list of regions for the specified country. The result is based on available region reports.'''

        return list(
            map(lambda file_name: _os.path.splitext(file_name)[0],
                self.__paths.get_country_regions_reports_paths(country_name)))

    def get_countries(self) -> _List[str]:
        ''' Returns list of all availables countries to process. '''
        return list(self.__paths.get_countries_reports_paths())

    def get_country_report(self,
                           country_name: str,
                           parse_dates=True,
                           date_is_index=True) -> _pd.DataFrame:
        '''Returns a country report as pandas DataFrame.

        Args:
            country_name(str): The name of the country for which report should be retrieved.
            parse_dates(bool): A flag indicating whether or not dates in DataFrame should be parsed. Default is True.
            date_is_index(bool): A flag indicating whether or not date column should be used as index. This parameter is ignored when 'parse_dates' is False. Default is True.

        Returns:
            DataFrame: a country report
        '''

        return _Storage.__read_csv_file(
            self.__paths.get_country_report_path(country_name), parse_dates,
            date_is_index)

    def get_region_report(self,
                          country_name: str,
                          region_name: str,
                          parse_dates=True,
                          date_is_index=True) -> _pd.DataFrame:
        '''Returns a country's region report as pandas DataFrame.

        Args:
            country_name(str): The name of the country for which report should be retrieved.
            region_name(str): The name of the country's region for which report should be retrieved.
            parse_dates(bool): A flag indicating whether or not dates in DataFrame should be parsed. Default is True.
            date_is_index(bool): A flag indicating whether or not date column should be used as index. This parameter is ignored when 'parse_dates' is False. Default is True.

        Returns:
            DataFrame: a country's region report
        '''

        return _Storage.__read_csv_file(
            self.__paths.get_region_report_path(country_name, region_name),
            parse_dates, date_is_index)

    @staticmethod
    def __read_csv_file(filepath: str, parse_dates,
                        date_is_index) -> _pd.DataFrame:
        columns_types = {
            'Confirmed': int,
            'Deaths': int,
            'Recovered': int,
            'Confirmed_Change': int,
            'Deaths_Change': int,
            'Recovered_Change': int,
            'Rt': float,
            'Time_To_Resolve': float
        }

        if (parse_dates):
            if (date_is_index):
                return _pd.read_csv(filepath,
                                    parse_dates=["Date"],
                                    index_col="Date",
                                    dayfirst=True,
                                    dtype=columns_types)
            else:
                return _pd.read_csv(filepath,
                                    parse_dates=["Date"],
                                    dayfirst=True,
                                    dtype=columns_types)
        else:
            return _pd.read_csv(filepath, dtype=columns_types)

    @staticmethod
    def __get_series_or_dataframe(df: _pd.DataFrame,
                                  name: str,
                                  column_name: str = None,
                                  start_date: _pd.Timestamp = None,
                                  wide_form: bool = True):

        column_series = {
            column_name and start_date and wide_form:
            lambda x: df.loc[start_date:, column_name],
            column_name and start_date and not wide_form:
            lambda x: df.loc[df.Date >= start_date, ['Date', column_name]],
            column_name and not start_date and wide_form:
            lambda x: df[column_name],
            column_name and not start_date and not wide_form:
            lambda x: df[['Date', column_name]],
            not column_name and start_date and wide_form:
            lambda x: df.loc[start_date:],
            not column_name and start_date and not wide_form:
            lambda x: df.loc[df.Date >= start_date],
            not column_name and not start_date and wide_form:
            lambda x: df,
            not column_name and not start_date and not wide_form:
            lambda x: df
        }[True](None)

        if wide_form:
            column_series = column_series.rename(name)
        else:
            column_series['Name'] = name

        return column_series

    def get_regions_report_by_column(self,
                                     country_name: str,
                                     column_name: str,
                                     include: _List[str] = None,
                                     exclude: _List[str] = None,
                                     start_date: _pd.Timestamp = None,
                                     wide_form: bool = True) -> _pd.DataFrame:
        ''' TBD '''

        regions_series = list()
        regions = include

        if (regions is None or len(regions) == 0):
            regions = self.get_country_regions(country_name)

        for region in regions:
            if (exclude and region in exclude):
                continue

            region_df = self.get_region_report(country_name,
                                               region,
                                               date_is_index=wide_form)

            regions_series.append(
                _Storage.__get_series_or_dataframe(region_df, region,
                                                   column_name, start_date,
                                                   wide_form))

        return _pd.concat(regions_series, axis=1 if wide_form else 0).fillna(0)

    def get_countries_report(
            self,
            include: _List[str] = None,
            exclude: _List[str] = None,
            start_date: _pd.Timestamp = None) -> _pd.DataFrame:
        ''' TBD '''

        countries_series = list()
        countries = include

        if (countries is None or len(countries) == 0):
            countries = self.get_countries()

        for country in countries:
            if (exclude and country in exclude):
                continue

            country_df = self.get_country_report(country, date_is_index=False)

            countries_series.append(
                _Storage.__get_series_or_dataframe(country_df,
                                                   country,
                                                   start_date=start_date,
                                                   wide_form=False))

        return _pd.concat(countries_series).fillna(0)

    def get_countries_report_by_column(
            self,
            column_name: str,
            include: _List[str] = None,
            exclude: _List[str] = None,
            start_date: _pd.Timestamp = None,
            wide_form: bool = True) -> _pd.DataFrame:
        ''' TBD '''

        countries_series = list()
        countries = include

        if (countries is None or len(countries) == 0):
            countries = self.get_countries()

        for country in countries:
            if (exclude and country in exclude):
                continue

            country_df = self.get_country_report(country,
                                                 date_is_index=wide_form)

            countries_series.append(
                _Storage.__get_series_or_dataframe(country_df, country,
                                                   column_name, start_date,
                                                   wide_form))

        return _pd.concat(countries_series,
                          axis=1 if wide_form else 0).fillna(0)

    def get_countries_stats(self) -> _pd.DataFrame:
        """
        docstring
        """

        report_file = self.__paths.get_countries_stats_path()
        stats_df = _pd.read_csv(report_file, index_col=["Name"])
        return stats_df.sort_index()

    def get_provinces_stats(self, country_name: str) -> _pd.DataFrame:

        report_file = self.__paths.get_country_regions_stats_path(country_name)
        stats_df = _pd.read_csv(report_file, index_col=["Name"])
        return stats_df.sort_index()

    def get_counties_stats(self, country_name: str,
                           region_name: str) -> _pd.DataFrame:

        report_file = self.__paths.get_country_counties_stats_path(
            country_name, region_name)
        stats_df = _pd.read_csv(report_file, index_col=["Name"])
        return stats_df.sort_index()
