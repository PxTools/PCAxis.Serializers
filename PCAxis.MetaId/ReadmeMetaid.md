# The Meta-id system
String replacement system to turn things like

"KORTNAVN:aku"

into 

"relation": "statistics-homepage"
"href": "https://www.ssb.no/befolkning/folketall/statistikk/befolkning"
"label": "Statistics homepage"
"type": "text/html"


using a file called metaid.config.

The usecase is to provide content in pxweb2gui for the areas shown in Figma_infomation_defs.png
The client is the mapper that maps a paxiom to jsonstat2 for the metadata endpoint in pxwebapi, which produces something like what is shown in example.json.


Figma_infomation_defs.png: 
  to get content for "area-1" , look for relation="statistics-homepage" on root
  to get content for "area-2" , look for relation="about-statistics" on root
  to get content for "area-3" , loop variables and look for relations starting with "definition-"
  
  