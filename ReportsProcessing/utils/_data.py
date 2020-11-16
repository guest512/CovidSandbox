import typing as _types
import pandas as _pd

Baseline = _types.Tuple[_types.Union[_pd.Timestamp,
                                     _types.Tuple[str, _pd.Timestamp]], int]


class _DataHelper:
    def __init__(self, storage):
        self._storage = storage

    @staticmethod
    def normalize(values: _pd.Series) -> _pd.Series:
        ''' Maps series of values to scale from -1.0 to 1.0 and returns it as a new Series object. '''
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
        '''
        Converts values to values-per-capita.

        Args:
            val(DataFrame | Series): Data to convert.
            country_name(str): The name of the country for which data should be converted. This is mandatory argument.
            province(str): The name of the province for which data should be converted. This is optional argument.
            county(str): The name of the county for which data should be converted. This is optional argument

        Returns:
            DataFrame or Series with data-per-capita.
        '''

        stats = None

        if province and county:
            stats = self._storage.get_counties_stats(country,
                                                     province).loc[county]
        elif province:
            stats = self._storage.get_regions_stats(country).loc[province]
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
        '''
        Converts values to values-per-<particular value>.

        Args:
            val(DataFrame | Series): Data to convert.
            country_name(str): The name of the country for which data should be converted. This is mandatory argument.
            province(str): The name of the province for which data should be converted. This is optional argument.
            county(str): The name of the county for which data should be converted. This is optional argument
            per(int): The value 

        Returns:
            DataFrame or Series with data-per-<particular value>.
        '''

        return self.per_capita(values, country, province, county) * per

    @staticmethod
    def set_baseline(
            values: _types.Union[_pd.DataFrame, _pd.Series],
            baseline: Baseline) -> _types.Union[_pd.DataFrame, _pd.Series]:
        '''
        Sets in a specified date and column (optionally) the particular value, and converts all other data accordingly.

        Args:
            val(DataFrame | Series): Data to convert.
            baseline((Timestamp, int) | ((str, Timestamp), int)): Coordinates in DataFrame or Series, and value that should be set in the coordinates.
            
        Returns:
            DataFrame or Series with converted data.
        '''
        baseline_value = baseline[1]
        actual_value = 0

        if isinstance(values, _pd.DataFrame):
            actual_value = values.loc[baseline[0][1], baseline[0][0]]
        else:
            actual_value = values.loc[baseline[0]]

        return values * baseline_value / actual_value

    @staticmethod
    def per_week(
        values: _types.Union[_pd.DataFrame, _pd.Series]
    ) -> _types.Union[_pd.DataFrame, _pd.Series]:
        '''
        Resamples values to per week frequency and makes it in the manner, 
        that allows to use utils.first_week, utils.last_week dates, and one_week interval as an index.

        Args:
            val(DataFrame | Series): Data to resample.

        Returns:
            DataFrame or Series resampled per week.
        '''
    
        return values.resample('W-MON', label='left', closed='left').sum()
