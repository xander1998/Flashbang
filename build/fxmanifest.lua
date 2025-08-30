game "gta5"
fx_version "cerulean"

author "Xander1998 (X. Cross) https://github.com/xander1998"
description "Flashbang script for FiveM"

--[[
	Scripted By: Xander1998 (X. Cross)
	Modified By: Eddie (Eddbox LP) & Local9
	Model Ported & META's redone By: xRxExTxRxOx
--]]


---------------------------------------------------------------------------
-- INCLUDED FILES
---------------------------------------------------------------------------
files {
	"data/**/*",
	"client/Newtonsoft.Json.dll",
}

---------------------------------------------------------------------------
-- DATA FILES
---------------------------------------------------------------------------
data_file "WEAPON_METADATA_FILE" "data/weaponarchetypes.meta"
data_file "WEAPON_ANIMATIONS_FILE" "data/weaponanimations.meta"
data_file "LOADOUTS_FILE" "data/loadouts.meta"
data_file "WEAPONINFO_FILE" "data/weapons.meta"
data_file "PED_PERSONALITY_FILE" "data/pedpersonality.meta"


---------------------------------------------------------------------------
-- SCRIPTS
---------------------------------------------------------------------------
server_script "server/Flashbang.Server.net.dll"
client_script "client/Flashbang.Client.net.dll"
