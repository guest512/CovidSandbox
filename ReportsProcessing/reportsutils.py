import os as os
import pandas as pd

__reports_path =  os.path.abspath("..\\bin\\Debug\\netcoreapp3.1\\reports\\countries")

def __get_country_regions_folder(country_name):
    return os.path.join(__reports_path,country_name, "regions")

def get_country_regions(country_name):
    return map(lambda file_name: os.path.splitext(file_name)[0], os.listdir(__get_country_regions_folder(country_name)))

def get_country_report(country_name, parse_dates=True, date_is_index = True):
    report_file = os.path.join(__reports_path, country_name, country_name+".csv")
    country_df = None

    if(parse_dates):
        if(date_is_index):
            country_df = pd.read_csv(report_file, parse_dates=["Date"], index_col = "Date", dayfirst=True)
        else:
            country_df = pd.read_csv(report_file, parse_dates=["Date"], dayfirst=True)
    else:
        country_df = pd.read_csv(report_file)

    return country_df

def get_region_report(country_name, region_name, parse_dates=True, date_is_index = True):
    report_file = os.path.join(__reports_path, country_name, "regions", region_name+".csv")
    region_df = None

    if(parse_dates):
        if(date_is_index):
            region_df = pd.read_csv(report_file, parse_dates=["Date"], index_col = "Date", dayfirst=True)
        else:
            region_df = pd.read_csv(report_file, parse_dates=["Date"], dayfirst=True)
    else:
        region_df = pd.read_csv(report_file)

    return region_df

def normalize(values):
    return values / values.max()

def draw_key_russian_dates_on_plot(ax):
    ax.axvline(pd.to_datetime('01-04-2020', dayfirst=True), label = 'Начало нерабочих дней', color = 'Red')
    ax.axvline(pd.to_datetime('12-05-2020', dayfirst=True), label = 'Пик пройден', color = 'Cyan')
    ax.axvline(pd.to_datetime('15-06-2020', dayfirst=True), label = 'Выход из эпидемии', color = 'Green')
    ax.axvline(pd.to_datetime('24-06-2020', dayfirst=True), label = 'Парад')
    ax.axvspan(pd.to_datetime('25-06-2020', dayfirst=True), pd.to_datetime('01-07-2020', dayfirst=True), label='Конституция', alpha = 0.2, color = 'Green')