--[[
	Scripted By: Xander1998 (X. Cross)
--]]

resource_manifest_version '44febabe-d386-4d18-afbe-5e627f4af937'

---------------------------------------------------------------------------
-- INCLUDED FILES
---------------------------------------------------------------------------
files {
	"data/loadouts.meta",
	"data/weaponanimations.meta",
	"data/weapons.meta"
}

---------------------------------------------------------------------------
-- DATA FILES
---------------------------------------------------------------------------
data_file "WEAPONINFO_FILE" "data/weapons.meta"
data_file "WEAPON_ANIMATIONS_FILE" "data/weaponanimations.meta"
data_file "LOADOUTS_FILE" "data/loadouts.meta"


---------------------------------------------------------------------------
-- SCRIPTS
---------------------------------------------------------------------------
server_script "Flashbang.Server.net.dll"
client_script "Flashbang.Client.net.dll"