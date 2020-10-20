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
            map(
                lambda file_name: _os.path.splitext(file_name)[0],
                _os.listdir(
                    self.__paths.get_country_regions_reports_path(
                        country_name))))

    def get_countries(self) -> _List[str]:
        ''' Returns list of all availables countries to process. '''
        return list(_os.listdir(self.__paths.get_countries_reports_path()))

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

        report_file = _os.path.join(
            self.__paths.get_country_reports_path(country_name),
            country_name + ".csv")
        country_df = None

        if (parse_dates):
            if (date_is_index):
                country_df = _pd.read_csv(report_file,
                                          parse_dates=["Date"],
                                          index_col="Date",
                                          dayfirst=True)
            else:
                country_df = _pd.read_csv(report_file,
                                          parse_dates=["Date"],
                                          dayfirst=True)
        else:
            country_df = _pd.read_csv(report_file)

        return country_df

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
        report_file = _os.path.join(
            self.__paths.get_country_regions_reports_path(country_name),
            region_name + ".csv")
        region_df = None

        if (parse_dates):
            if (date_is_index):
                region_df = _pd.read_csv(report_file,
                                         parse_dates=["Date"],
                                         index_col="Date",
                                         dayfirst=True)
            else:
                region_df = _pd.read_csv(report_file,
                                         parse_dates=["Date"],
                                         dayfirst=True)
        else:
            region_df = _pd.read_csv(report_file)

        return region_df

    def get_regions_report_by_column(
            self,
            country_name: str,
            column_name: str,
            include: _List[str] = None,
            exclude: _List[str] = None,
            start_date: _pd.Timestamp = None) -> _pd.DataFrame:
        ''' TBD '''

        regions_series = list()
        regions = include

        if (regions is None or len(regions) == 0):
            regions = self.get_country_regions(country_name)

        for region in regions:
            if (exclude is not None and region in exclude):
                continue

            region_df = self.get_region_report(country_name, region)
            column_series = None

            if (start_date is not None):
                column_series = region_df.loc[start_date:, column_name]
            else:
                column_series = region_df[column_name]

            regions_series.append(column_series.rename(region))

        return _pd.concat(regions_series, axis=1).fillna(0)

    def get_countries_report_by_column(
            self,
            column_name: str,
            include: _List[str] = None,
            exclude: _List[str] = None,
            start_date: _pd.Timestamp = None) -> _pd.DataFrame:
        ''' TBD '''

        countries_series = list()
        countries = include

        if (countries is None or len(countries) == 0):
            countries = self.get_countries()

        for country in countries:
            if (exclude is not None and country in exclude):
                continue

            country_df = self.get_country_report(country)
            column_series = None

            if (start_date is not None):
                column_series = country_df.loc[start_date:, column_name]
            else:
                column_series = country_df[column_name]

            countries_series.append(column_series.rename(country))

        return _pd.concat(countries_series, axis=1).fillna(0)
