# The Meta-id system
String replacement system to turn things like

"KORTNAVN:aku"

into 
```
"relation": "statistics-homepage"
"href": "https://www.ssb.no/en/aku"
"label": "Statistics homepage"
"type": "text/html"
```

using a file called metaid.config. There a fragment:
```
    <metaSystem id="KORTNAVN">
      ...
      <links type="text/html" relation="statistics-homepage">
        ...
        <link px-lang="en" labelStringFormat="Statistics homepage" urlStringFormat="https://www.ssb.no/en/{0}" />
      </links>
    </metaSystem> 
``` 

The usecase is to provide content in pxweb2gui for the areas shown in Figma_infomation_defs.png

  ![Information pane in figma](Figma_infomation_defs.png?raw=true)


The client is the mapper that maps a paxiom to jsonstat2 for the metadata endpoint in pxwebapi, which produces something like what is shown in example.json.


Figma_infomation_defs.png: 
  to get content for "area-1" , look for relation="statistics-homepage" on root
  to get content for "area-2" , look for relation="about-statistics" on root
  to get content for "area-3" , loop variables and look for relations starting with "definition-"
  
  
