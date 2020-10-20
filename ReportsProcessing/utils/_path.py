import os as _os


class __PathHelper():
    def __get_data_path(self) -> str:
        return _os.path.abspath(_os.path.join(_os.path.dirname(__file__), "..", "data"))

    def __get_reports_path(self) -> str:
        return _os.path.join(self.__get_data_path(), "reports")

    def get_stats_path(self) -> str:
        return _os.path.join(self.__get_data_path(), "stats")

    def get_countries_reports_path(self) -> str:
        return _os.path.join(self.__get_reports_path(), "countries")

    def get_daily_reports_path(self) -> str:
        return _os.path.join(self.__get_reports_path(), "dayByDay")

    def get_country_reports_path(self, country: str) -> str:
        return _os.path.join(self.get_countries_reports_path(), country)

    def get_country_regions_reports_path(self, country: str) -> str:
        return _os.path.join(self.get_country_reports_path(country), "regions")
