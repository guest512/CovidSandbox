import time as _time
import locale as _locale
import contextlib as _ctxlib
import pandas as _pd


def normalize(values: _pd.Series) -> _pd.Series:
    ''' Maps series of values to scale from 0.0 to 1.0 and returns it as a new Series object '''
    min = values.min()
    max = values.max()

    return (values - min) / (max - min)


@_ctxlib.contextmanager
def setlocale_ctx(locale: str):
    saved = _locale.setlocale(_locale.LC_ALL)
    yield _locale.setlocale(_locale.LC_ALL, locale)
    _locale.setlocale(_locale.LC_ALL, saved)
