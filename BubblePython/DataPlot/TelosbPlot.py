import os
from datetime import datetime
import matplotlib.pyplot as plt
os.chdir(os.getenv('HOME')+"/Dropbox/LowEx-BubbleZERO/Bubble Data")
f = open('CO2flap31.txt')
sourcedata = list()
for line in f:
    sourcedata.append(line)
time = list();
temperature = list()
CO2Concentration = list()
for element in sourcedata:
    element = str(element)
    element = element.split(';')
    if len(element) == 3:
        time.append(datetime.strptime(element[0],'%Y-%m-%d %H:%M:%S'))
        temperature.append(float(element[1]))
        CO2Concentration.append(float(element[2]))
plt.figure(1)
plt.subplot(211)
line1 = plt.plot_date(time, temperature,'b*')
#plt.setp(line1,animated='True')
plt.subplot(212)
plt.plot_date(time,CO2Concentration,'r^')
plt.show()