// Building Blacklist
// file:          Server_BuildBlacklist.cs
// author:        Peggworth the Pirate
//======================================================================================


// check if the user is banned
function BannedBuilding(%client)
{
	%blid = %client.bl_id;
	for( %i=0; %i < getWordCount($Pref::Server::BuildBlacklist); %i++ )
	{
		if( getWord($Pref::Server::BuildBlacklist, %i) == %blid )
			return 1;
	}
	return 0;
}

// ban a user
function BanBuildRights(%what)
{
	%client = isObject(findClientByName(%what)) ? findClientByName(%what) : findClientByBL_ID(%what);
	if( %client.isHost || BannedBuilding(%client) || !isObject(%client) )
		return 0;
	%blid = %client.bl_id;
	$Pref::Server::BuildBlacklist = ($Pref::Server::BuildBlacklist $= "") ? %blid : $Pref::Server::BuildBlacklist SPC %blid;
	return 1;
}

// un-ban a user
function RemoveBuildBan(%what)
{
	%client = isObject(findClientByName(%what)) ? findClientByName(%what) : findClientByBL_ID(%what);
	if( !BannedBuilding(%client) || %client.isHost || !isObject(%client) )
		return 0;
	%blid = %client.bl_id;
	for( %i=0; %i < getWordCount($Pref::Server::BuildBlacklist); %i++ )
	{
		%listBLID = getWord($Pref::Server::BuildBlacklist,%i);
		if( %listBLID != %blid )
			%newList = (%newList $= "") ? %listBLID : %newList SPC %listBLID;
	}
	$Pref::Server::BuildBlacklist = %newList;
	return 1;
}
	
deactivatePackage(BuildingBlackList);
package BuildingBlackList
{
	// prevent users who are banned from building
	function serverCmdPlantBrick(%client)
	{
		if ( %client.isHost )	// host can build no matter what
			return parent::serverCmdPlantBrick(%client);
		
		switch ( $Pref::Server::BuildingBlacklist::BannedAdminsBuild )
		{
			case 1:
				if( BannedBuilding(%client) && !%client.isAdmin )
				{
					centerPrint(%client,"\c0You need \c6Building Rights\c0 to build on this Server.<br>\c3(You have been banned from building)",3,3);
					return;
				}
			case 0:
				if( BannedBuilding(%client) )
				{
					centerPrint(%client,"\c0You need \c6Building Rights\c0 to build on this Server.<br>\c3(You have been banned from building)",3,3);
					return;
				}
		}
		return parent::serverCmdPlantBrick(%client);
	}
};
