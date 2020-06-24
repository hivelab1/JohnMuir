This directory contains EXAMPLE json data that reflects internal app structure. To use these:

1) Copy them somewhere else.
2) Change them to your liking.
3) Upload them somewhere. 
4) Set the path to retrieve the manifest:
	4a) Open the editor.
	4b) Open the UI scene.
	4c) Select "Canvas", and locate "UI" script.
	4d) OUTSIDE OF PLAY MODE: paste the path to the manifest into "Manifest URI" field in script via Editor.
5) When you launch the app, it will retrieve the URI you just set, and look for a manifest.json. Assuming you set the right path,
the app should retreive the remote manifest.json. It will systematically crawl the manifest.json URIs field to locate all your 
JSON assets and retrieve and load them. 
6) If you did this right, your app should automatically populate with data upon launch. TEST THIS BEFORE BUILDING.
7) You're a hero, and you're done!

IMPORTANT:
	DO NOT MODIFY 'instanceID' OBJECTS IN JSON. THESE ARE FOR INTERNAL USE AND MUST BE IN THE FILE, BUT ARE NOT TO BE ALTERED OR ASSIGNED BY USERS.