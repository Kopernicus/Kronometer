# Kronometer
A mod to manipulate the clock for Kerbal Space Program. It was previously a part of Kopernicus (called ClockFixer), but it was moved into its own mod.

### Copyright
* Thomas P.
* Sigma88

This mod is licensed under the Terms of the MIT License. It was previously licensed under the LGPL License.

## Downloads

- [Latest Release](https://github.com/StollD/Kronometer/releases/latest)

### Credits
Sigma88 wrote most of the original code, Thomas P. made various improvements. The name is an idea of the IRC user "acc"

This uses the [Kopernicus parsing and attribution library](https://github.com/Kopernicus/config-parser) written by Nathaniel R. Lewis (Teknoman117), which is LGPL


# Kronometer Syntax

```
Kronometer
{
}
```
This is the root node, it is provided by the mod and should be edited using [ModuleManager](http://forum.kerbalspaceprogram.com/index.php?/topic/50533-0/) patches.

The Kronometer root node contains five general settings and three nodes for more specific changes:

- **useHomeDay**

  *Boolean, default value = false*

  > When *'true'* the duration of a 'day' will be changed to match 1 solar day of the home planet.

- **useHomeYear**

  *Boolean, default value = false*

  > When *'true'* the duration of a 'year' will be changed to match the orbital period of the home planet.

- **useLeapYears**

  *Boolean, default value = false*

  >  - **When *'false'***:
  >
  >    if the year ends halfway through a day, the clock will go:
  >
  >    Year 1 Day 365   ==>   Year 2 Day 0    (Instead of starting directly with Day 1)
  >
  >    Day 0 will last untill Day 365 would have ended, then Day 1 will start.
  >
  >  - **When *'true'***:
  >
  >    at the end of a day, if there is not enough time left in the year to fit another full day,
  >
  >    the current year will end as well, and the new day will be the first day of the next year
  >
  >
  >  - **In both cases**: a new day will not start earlier just because a year has ended.
  >  

- **resetMonthNumAfterMonths**

  *Integer, default value set to match total number of months*
  
  > The number assigned to each month can go up to this number. If the number of months is higher than this value, some months will have the same number.
  
 - **resetMonthsAfterYears**

   *Integer, default value = 1*
  
   > This number defines how many years can pass before the calendar starts over from the first month.
  
  ---
  
 - **CustomTime**
   ```
   Kronometer
   {
       CustomTime
       {
           Year
           {
           }
           Day
           {
           }
           Hour
           {
           }
           Minute
           {
           }
           Second
           {
           }
       }
   }
   ```
   This is used to change the definitions of the five units of time used by KSP: *Year*, *Day*, *Hour*, *Minute* and *Second*.
   
   - **singular**, *\<string\>*, which is the singular name of the time unit (eg. *'Year'*)
   - **plural**, *\<string\>*, which is the plural name of the time unit (eg. *'Years'*)
   - **symbol**, *\<string\>*, which is the symbol used for the time unit (eg. *'y'*)
   - **value**, *\<double\>*, which is the duration in 'real life seconds' of the time unit (eg. *'9201600'*)
   - **roundToNearestInt**, *\<bool\>*, which (if true) makes sure the 'value' is an integer number
  
  ---
  
 - **Months**
   ```
   Kronometer
   {
       Months
       {
           Month
           {
           }
           Month
           {
           }
           Month
           {
           }
       }
   }
   ```
   This is used to define months that can be used when displaying the date.
 
   All months require:
   - **name**, *\<string\>*, which is the full name of the month (eg. *'January'*)
   - **symbol**, *\<string\>*, which is the symbol used for the month (eg. *'Jan'*)
   - **days**, *\<int\>*, which is the number of days in the month (eg. *'30'*)
   
  ---
  
 - **DisplayDate**
   ```
   Kronometer
   {
       DisplayDate
       {
           PrintDate
           {
           }
           PrintDateNew
           {
           }
           PrintDateCompact
           {
           }
       }
   }
   ```
   This is used to change how KSP displays the date when using
   [*PrintDate*](https://kerbalspaceprogram.com/api/interface_i_date_time_formatter.html#a1925dd76af3a9a62ff77cfa0075da03f), 
   [*PrintDateNew*](https://kerbalspaceprogram.com/api/interface_i_date_time_formatter.html#a9f178261dbd9ecd419325690631db4fc) 
   and 
   [*PrintDateCompact*](https://kerbalspaceprogram.com/api/interface_i_date_time_formatter.html#ae79d6114f4ae8a175d26a5d676e5c0a9).

   All three nodes can contain the following parameters:
   - **offsetTime**, *\<double\>*, changes the time provided by KSP ***before*** calculating the current date
   - **offsetYear**, *\<int\>*, changes the year number ***after*** having calculated the date
   - **offsetDay**, *\<int\>*, changes the day number ***after*** having calculated the date
   - **displayDate**, *\<string\>*, which is the format used to display the date
   - **displayTime**, *\<string\>*, which is added to ***'displayDate'*** when KSP provides *'includeTime = true'*
   - **displaySeconds**, *\<string\>*, which is added to ***'displayDate'*** when KSP provides *'includeSeconds = true'*
  
   **Custom Format:**
  
   **displayDate**, **displayTime** and **displaySeconds** use
   ***[.NET Framework composite formatting](https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx)***
   with a few minor changes:
   - angle brackets (```<``` and ```>```) are used instead of curly brackets (```{``` and ```}```)
   - ```<Y>```, ```<D>```, ```<H>```, ```<M>```, ```<S>``` are used to get the **current number** of the chosen unit of time
   - ```<Y0>```, ```<D0>```, ```<H0>```, ```<M0>```, ```<S0>``` are used to get the **symbol** of the chosen unit of time
   - ```<Y1>```, ```<D1>```, ```<H1>```, ```<M1>```, ```<S1>``` are used to get the **singular** of the chosen unit of time
   - ```<Y2>```, ```<D2>```, ```<H2>```, ```<M2>```, ```<S2>``` are used to get the **plural or singular** of the chosen unit of time <sub><sup>(depending on the date)</sup></sub>
   - ```<Mo>```, ```<Mo0>```, ```<Mo1>``` are used to get **number**, **symbol** or **name** of the current month
   - ```<Dth>``` is used to get the cardinal suffix (```st```, ```nd```, ```rd``` or ```th```) for the number of the current day
   - to add an angle bracket, use two of the same back to back (```<<``` to get ```<```, and ```>>``` to get ```>```)
   - to add spaces at the beginning or end of the string, open an angle bracket and add a space (```< ``` to get '``` ```')
