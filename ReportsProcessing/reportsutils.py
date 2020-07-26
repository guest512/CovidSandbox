import os as __os
import pandas as __pd
from typing import List as _List
from matplotlib import axes as _axes

__reports_path = __os.path.abspath(
    "..\\bin\\Debug\\netcoreapp3.1\\reports\\countries")


def __get_country_regions_folder(country_name: str) -> str:
    '''Returns path to folder 'regions' for specified country.'''

    return __os.path.join(__reports_path, country_name, "regions")


def get_country_regions(country_name: str) -> _List[str]:
    '''Returns list of regions for the specified country. The result is based on available region reports.'''

    return map(lambda file_name: __os.path.splitext(file_name)[0], __os.listdir(__get_country_regions_folder(country_name)))


def get_countries():
    ''' Returns list of all availables countries to process. '''
    return __os.listdir(__reports_path)


def get_country_report(country_name: str, parse_dates=True, date_is_index=True) -> __pd.DataFrame:
    '''Returns a country report as pandas DataFrame.

    Args:
        country_name(str): The name of the country for which report should be retrieved.
        parse_dates(bool): A flag indicating whether or not dates in DataFrame should be parsed. Default is True.
        date_is_index(bool): A flag indicating whether or not date column should be used as index. This parameter is ignored when 'parse_dates' is False. Default is True.

    Returns:
        DataFrame: a country report
    '''

    report_file = __os.path.join(
        __reports_path, country_name, country_name+".csv")
    country_df = None

    if(parse_dates):
        if(date_is_index):
            country_df = __pd.read_csv(report_file,
                                       parse_dates=["Date"], index_col="Date", dayfirst=True)
        else:
            country_df = __pd.read_csv(report_file,
                                       parse_dates=["Date"], dayfirst=True)
    else:
        country_df = __pd.read_csv(report_file)

    return country_df


def get_region_report(country_name: str, region_name: str, parse_dates=True, date_is_index=True) -> __pd.DataFrame:
    '''Returns a country's region report as pandas DataFrame.

    Args:
        country_name(str): The name of the country for which report should be retrieved.
        region_name(str): The name of the country's region for which report should be retrieved.
        parse_dates(bool): A flag indicating whether or not dates in DataFrame should be parsed. Default is True.
        date_is_index(bool): A flag indicating whether or not date column should be used as index. This parameter is ignored when 'parse_dates' is False. Default is True.

    Returns:
        DataFrame: a country's region report
    '''
    report_file = __os.path.join(
        __reports_path, country_name, "regions", region_name+".csv")
    region_df = None

    if(parse_dates):
        if(date_is_index):
            region_df = __pd.read_csv(report_file,
                                      parse_dates=["Date"], index_col="Date", dayfirst=True)
        else:
            region_df = __pd.read_csv(report_file,
                                      parse_dates=["Date"], dayfirst=True)
    else:
        region_df = __pd.read_csv(report_file)

    return region_df


def normalize(values: __pd.Series) -> __pd.Series:
    ''' Maps series of values to scale from 0.0 to 1.0 and returns it as a new Series object '''

    return values / values.max()


def draw_key_russian_dates_on_plot(ax: _axes.Axes):
    ''' Draws key dates from Russia on plot as vertical lines and spans. '''

    ax.axvline(__pd.to_datetime('01-04-2020', dayfirst=True),
               label='Путин: Начало нерабочих дней', color='Red')
    ax.axvline(__pd.to_datetime('12-05-2020', dayfirst=True),
               label='Путин: Пик пройден', color='Cyan')
    ax.axvline(__pd.to_datetime('15-06-2020', dayfirst=True),
               label='Путин: Выход из эпидемии', color='Green')
    ax.axvline(__pd.to_datetime('24-06-2020', dayfirst=True), label='Парад')
    ax.axvspan(__pd.to_datetime('25-06-2020', dayfirst=True),
               __pd.to_datetime('01-07-2020', dayfirst=True), label='Конституция', alpha=0.2, color='Green')


def get_regions_report_by_column(country_name: str, column_name: str, include: _List[str] = None, exclude: _List[str] = None, start_date: __pd.datetime = None) -> __pd.DataFrame:
    ''' TBD '''

    regions_df = __pd.DataFrame()
    regions = include

    if(regions is None or len(regions) == 0):
        regions = get_country_regions(country_name)

    for region in regions:
        if(exclude is not None and region in exclude):
            continue

        region_df = get_region_report(country_name, region)

        if(start_date is not None):
            region_df = region_df.loc[start_date:, column_name]
        else:
            region_df = region_df[column_name]

        regions_df[region] = region_df

    return regions_df
