import typing as _types
import pandas as _pd

Baseline =  _types.Tuple[_types.Union[_pd.Timestamp,_types.Tuple[str,_pd.Timestamp]], int]

class _DataHelper:
    def __init__(self, storage):
        self._storage = storage

    @staticmethod
    def normalize(values: _pd.Series) -> _pd.Series:
        ''' Maps series of values to scale from -1.0 to 1.0 and returns it as a new Series object '''
        min = values.abs().min()
        max = values.abs().max()

        return (values - min) / (max - min)

    def per_capita(
        self,
        values: _types.Union[_pd.DataFrame, _pd.Series],
        country: str,
        province: _types.Optional[str] = None,
        county: _types.Optional[str] = None
    ) -> _types.Union[_pd.DataFrame, _pd.Series]:
        stats = None

        if province and county:
            stats = self._storage.get_counties_stats(country,
                                                     province).loc[county]
        elif province:
            stats = self._storage.get_provinces_stats(country).loc[province]
        else:
            stats = self._storage.get_countries_stats().loc[country]

        population = stats['Population']
        return values / population

    def per_value(self,
                   values: _types.Union[_pd.DataFrame, _pd.Series],
                   country: str,
                   province: _types.Optional[str] = None,
                   county: _types.Optional[str] = None,
                   per: int = 1000):
        return self.per_capita(values, country, province, county) * per

    @staticmethod
    def set_baseline(values: _types.Union[_pd.DataFrame, _pd.Series], baseline: Baseline) -> _types.Union[_pd.DataFrame, _pd.Series]:
        baseline_value = baseline[1]
        actual_value = 0
        
        if isinstance(values,_pd.DataFrame):
            actual_value = values.loc[baseline[0][1], baseline[0][0]]
        else:
            actual_value = values.loc[baseline[0]]

        return values * baseline_value / actual_value


