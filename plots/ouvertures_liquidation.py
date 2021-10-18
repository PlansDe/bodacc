import pandas as pd
import matplotlib.pyplot as plt

# TODO: parse dates properly (better : insert well formated dates in DB)
df = pd.read_csv('time_series.csv', parse_dates=['DATE'])
print(df.tail(20))
