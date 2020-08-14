import pandas as _pd


def normalize(values: _pd.Series) -> _pd.Series:
    ''' Maps series of values to scale from 0.0 to 1.0 and returns it as a new Series object '''

    return values / values.max()
