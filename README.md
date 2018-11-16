# Sitecore-Layout-Modes-Comparer
Sitecore Module "Layout Modes Comparer" highlights the difference between Sitecore Final and Shared Layouts and allows to reset the specific component, changed in the final layout.

 
# Problem:
 
There is no abality to compare Final and Shared layouts. 
  

# Features of the Module: 

The changes are shown in the Sitecore Message:
Click Presentation in shared and final layout to see the summary information obout the changes in the final layout
 
1. Highlight the changed components:
In the shared layout the module highlights the components updated or deleted on the final:
renderings, highlited with red board are deleted; with Yellow - updated
 
2. In the final layout the module highlights only new or updated components:
Blue – new and yellow as usual 😊
 
3. ToolTips show detailed information about every component’s changes:
- changed datasource item
- the components properties, updated in the final layout
- component is moved into another placeholder
  
4. Allow to undo the changes in the specific component with using the ExperienceEdit Button “Undo Components Changes”
  
Important:
[ module compares the final with shared layouts, and not the final layout with its standard values.]
  
 
# Install and Configure the module 
 

1. Install the module:
You can find the Layout Modes Comparer on the Sitecore MarketPlace:
https://github.com/maxim88/Sitecore-Layout-Modes-Comparer/blob/master/Layout%20Modes%20Comparer-1.01.zip or https://marketplace.sitecore.net/Modules/L/Layout_Modes_Comparer.aspx

When you download it, you will get a Sitecore Package that can be installed with the Sitecore Package Installer:
 
2. Activate the module:
The module extends the “SelectLayout” logic. That’s why the first step is to go to the folder “sitecore\shell\client\Sitecore\ExperienceEditor\Commands”, to deactivate the Sitecore “SelectLayout.js” file and to activate the “SelectLayout.js_layoutModesComparer” (deleting the suffix “_layoutModesComparer”).
  
3. Assign "Undo Components Changes" button on Renderings
 
   
# The module settings:
The changed components are highlighted with different colors:

blue – new component
red – deleted
yellow – updated
The colors can be configured in the Core DB in the configuration Item “/sitecore/system/Modules/Layout Modes Comparer/Color Settings”

 

 Enjoy 😊
