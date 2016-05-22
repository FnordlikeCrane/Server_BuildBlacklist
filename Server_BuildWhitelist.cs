// Building Whitelist
// file:          Server_BuildWhitelist.cs
// author:        Peggworth the Pirate
//======================================================================================


// check if the user is banned
function BuildingRights(%client)
{
	%blid = %client.bl_id;
	for( %i=0; %i < getWordCount($Pref::Server::BuildWhitelist); %i++ )
	{
		if( getWord($Pref::Server::BuildWhitelist, %i) == %blid )
			return 1;
	}
	return 0;
}

// give rights
function GrantBuildRights(%what)
{
	%client = isObject(findClientByName(%what)) ? findClientByName(%what) : findClientByBL_ID(%what);
	if( BuildingRights(%client) || !isObject(%client) )
		return 0;
	%blid = %client.bl_id;
	$Pref::Server::BuildWhitelist = ($Pref::Server::BuildWhitelist $= "") ? %blid : $Pref::Server::BuildWhitelist SPC %blid;
	return 1;
}

// remove rights
function RemoveBuildRights(%what)
{
	%client = isObject(findClientByName(%what)) ? findClientByName(%what) : findClientByBL_ID(%what);
	if( !BuildingRights(%client) || %client.isHost || !isObject(%client) )
		return 0;
	%blid = %client.bl_id;
	for( %i=0; %i < getWordCount($Pref::Server::BuildWhitelist); %i++ )
	{
		%listBLID = getWord($Pref::Server::BuildWhitelist,%i);
		if( %listBLID != %blid )
			%newList = (%newList $= "") ? %listBLID : %newList SPC %listBLID;
	}
	$Pref::Server::BuildWhitelist = %newList;
	return 1;
}

deactivatePackage(BuildingWhitelist);
package BuildingWhitelist
{	
	// prevent users who are banned from building
	function serverCmdPlantBrick(%client)
	{
		if ( %client.isHost )	// host can build no matter what
		{
			echo("host");
			return parent::serverCmdPlantBrick(%client);
		}
		switch ( $Pref::Server::BuildingBlacklist::BannedAdminsBuild )
		{
			case 1:
				if( !BuildingRights(%client) && !%client.isAdmin )
				{
					centerPrint(%client,"\c0You need \c6Building Rights\c0 to build on this Server.<br>\c3(You have been banned from building)",3,3);
					return;
				}
			case 0:
				if( !BuildingRights(%client) )
				{
					centerPrint(%client,"\c0You need \c6Building Rights\c0 to build on this Server.<br>\c3(You have not been granted rights)",3,3);
					return;
				}
		}
		return parent::serverCmdPlantBrick(%client);
	}
};
