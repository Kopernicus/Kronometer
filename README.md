# Kronometer
A mod to manipulate the clock for Kerbal Space Program. It was previously a part of Kopernicus (called ClockFixer), but it was moved into its own mod.

### Copyright
* Thomas P.
* Sigma88

This mod is licensed under the Terms of the MIT License. It was previously licensed under the LGPL License.

### Credits
Sigma88 wrote most of the original code, Thomas P. made various improvements. The name is an idea of the IRC user "acc"

This uses the [Kopernicus parsing and attribution library](https://github.com/Kopernicus/config-parser) written by Nathaniel R. Lewis (Teknoman117), which is LGPL


# Syntax

## Kronometer
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
  >    if the year ends halfway through a day, the clock will go:
  >
  >    Year 1 Day 365   ==>   Year 2 Day 0    (Instead of starting directly with Day 1)
  >
  >    Day 0 will last untill Day 365 would have ended, then Day 1 will start.
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
  
  
 
 
 
 
 
 
 
