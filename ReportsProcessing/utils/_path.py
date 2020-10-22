import typing as _types
import os as _os


class __PathHelper():
    def __init__(self):
        root = _os.path.abspath(
            _os.path.join(_os.path.dirname(__file__), "..", "data"))
        self._countries_root = _os.path.join(root, "reports", "countries")
        self._daily_root = _os.path.join(root, "reports", "dayByDay")
        self._stats_root = _os.path.join(root, "stats")

    def get_country_report_path(self, country: str) -> str:
        return _os.path.join(self._countries_root, country, country + ".csv")

    def get_region_report_path(self, country: str, province: str) -> str:
        return _os.path.join(self._countries_root, country, "regions",
                             province + ".csv")

    def get_countries_reports_paths(self) -> _types.List[str]:
        return _os.listdir(self._countries_root)

    def get_daily_reports_paths(self) -> _types.List[str]:
        return _os.listdir(self._daily_root)

    def get_country_regions_reports_paths(
            self, country: str) -> _types.List[str]:
        return _os.listdir(
            _os.path.join(self._countries_root, country, "regions"))

    def get_countries_stats_path(self) -> str:
        return _os.path.join(self._stats_root, "countries.csv")

    def get_country_regions_stats_path(self, country: str) -> str:
        return _os.path.join(self._stats_root, country, "regions.csv")

    def get_country_counties_stats_path(self, country: str,
                                        province: str) -> str:
        return _os.path.join(self._stats_root, country, province,
                             "counties.csv")
