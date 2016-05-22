// Building Blacklist
// file:          server.cs
// author:        Peggworth the Pirate
// contributors:  Mo / Kreg (Tester)
//--------------------------------------------------------------------------------------
//    Versions:
//          1.0.0:  Building can be banned							[?/??/2013]
//			1.1.0:	Building can now be banned via name or BL_ID	[5/12/2016]
//			1.2.0:	Server preferences have been added				[5/15/2016]
//			1.3.0:	New commands: /listbuildbans /clearbans
//					Ability to switch to whitelist mode				[5/16/2016]
//			1.3.1:	Revamped command system							[5/16/2016]
//======================================================================================


//--------------------------------------------------------------------------------------
//	Initiate preferences & execute files

if(isFile("Add-Ons/System_ReturnToBlockland/server.cs"))
{
	if(!$RTB::RTBR_ServerControl_Hook) 
		exec("Add-Ons/System_ReturnToBlockland/RTBR_ServerControl_Hook.cs");
	RTB_registerPref("Build Rights Privilege", "Building Rights","$Pref::Server::BuildingBlacklist::AdminRequirement", "List Admin 1 Super-Admin 2 Host 3", "Server_BuildBlacklist", 1, 0, 0);
	RTB_registerPref("Admins Can Still Build", "Building Rights", "$Pref::Server::BuildingBlacklist::BannedAdminsBuild", "bool","Server_BuildBlacklist", 1, 0, 0);
}
else
{
	if ( $Pref::Server::BuildingBlacklist::AdminRequirement $= "" ) $Pref::Server::BuildingBlacklist::AdminRequirement = 1;
	if ( $Pref::Server::BuildingBlacklist::BannedAdminsBuild $= "" ) $Pref::Server::BuildingBlacklist::BannedAdminsBuild = 1;
}
if ( $Pref::Server::BuildingBlacklist::WhitelistMode $= "" ) $Pref::Server::BuildingBlacklist::WhitelistMode = 0;

exec("./Server_BuildBlacklist.cs");
exec("./Server_BuildWhitelist.cs");

if ( !$Pref::Server::BuildingBlacklist::WhitelistMode )
	activatePackage(BuildingBlackList);
else
	activatePackage(BuildingWhiteList);

//======================================================================================
//	White/Blacklist toggle

function ToggleWhitelist()
{
	$Pref::Server::BuildingBlacklist::WhitelistMode = !$Pref::Server::BuildingBlacklist::WhitelistMode;	
	if ( $Pref::Server::BuildingBlacklist::WhitelistMode )
	{
		deactivatePackage(BuildingBlackList);
		activatePackage(BuildingWhiteList);
		chatMessageAll('',"\c6Building Whitelist is now enabled, you must now obtain building rights to build.");
	}
	else
	{
		activatePackage(BuildingBlackList);
		deactivatePackage(BuildingWhiteList);	
		chatMessageAll('',"\c6Building Blacklist is now enabled, everyone can build unless you're banned.");
	}
}


//======================================================================================
//	server command wrapped up into one bamboozler

// server command to ban
function serverCmdbuilding(%client, %what, %who)
{
	switch ( $Pref::Server::BuildingBlacklist::AdminRequirement )
	{
		case 1:
			if( !%client.isAdmin )
			{	
				messageClient(%client,'',"\c0You must be admin to use this command.");
				return;
			}		
		case 2:
			if( !%client.isSuperAdmin )
			{	
				messageClient(%client,'',"\c0You must be super-admin to use this command.");
				return;
			}
		case 3:
			if( !%client.isHost )
			{	
				messageClient(%client,'',"\c0You must host to use this command.");
				return;
			}		
	}

	%to = isObject(findClientByName(%who)) ? findClientByName(%who) : findClientByBL_ID(%who);
	switch$ ( %what )
	{
	
	// taking rights away from players
	case "takeRights":
		// during whitelistmode
		if ( $Pref::Server::BuildingBlacklist::WhitelistMode )
		{
			if( RemoveBuildRights(%who) )
			{
				messageClient(%to,'','\c0Your Building Rights have been revoked by \c6%1',%client.name);
				messageClient(%client,'','\c0You have removed the Building Rights for \c6%1',%to.name);
			}
			else
			{
				if ( isObject(%to) )
				{
					if ( %to.isHost )
						messageClient(%client,'',"\c0Error: Player is un-bannable.");
					else if ( !RemoveBuildRights(%who) )
						messageClient(%client,'',"\c0Error: Player does not have rights.");
				}
				else
					messageClient(%client,'',"\c0Error: Player not found.");
			}
		}
		// during blacklistmode
		else
		{
			if( BanBuildRights(%who) )
			{
				messageClient(%to,'','\c0Your Building Rights have been revoked by \c6%1',%client.name);
				messageClient(%client,'','\c0You have removed Building Rights for \c6%1',%to.name);
			}
			else
			{
				if ( isObject(%to) )
				{
					if ( %to.isHost )
						messageClient(%client,'',"\c0Error: Player is un-bannable.");
					else if ( !BanBuildRights(%who) )
						messageClient(%client,'',"\c0Error: Player is already banned.");
				}
				else
					messageClient(%client,'',"\c0Error: Player not found.");
			}
		}
	
	// giving rights to players
	case "giveRights":
		if ( $Pref::Server::BuildingBlacklist::WhitelistMode )
		{
			if( GrantBuildRights(%who) )
			{
				messageClient(%to,'','\c0Your Building Rights have been granted by \c6%1',%client.name);
				messageClient(%client,'','\c0You have granted Building Rights to \c6%1',%to.name);
			}
			else
			{
				if ( isObject(%to) && !GrantBuildRights(%who))
				{
					messageClient(%client,'',"\c0Error: Player already has Building Rights.");
				}
				else
					messageClient(%client,'',"\c0Error: Player not found.");
			}		
		}
		else 
		{
			if( RemoveBuildBan(%who) )
			{
				messageClient(%to,'','\c0Your Building Ban has been revoked by \c6%1',%client.name);
				messageClient(%client,'','\c0You have removed the Building Ban for \c6%1',%to.name);
			}
			else
			{
				if ( isObject(%to) && !RemoveBuildBan(%who) )
					messageClient(%client,'',"\c0Error: Player is not banned.");
				else
					messageClient(%client,'',"\c0Error: Player not found.");
			}
		}
		
	// list who has rights and who doesn't
	case "listRights":
		%any = 0;
		if ( $Pref::Server::BuildingBlacklist::WhitelistMode )
		{
			for( %i=0; %i < getWordCount($Pref::Server::BuildWhitelist); %i++ )
			{
				%banned = getWord($Pref::Server::BuildWhitelist, %i);
				%show = isObject(findClientByBL_ID(%banned)) ? findClientByBL_ID(%banned).name : "BL_ID " @ %banned;
				if ( %show !$= "" )
				{
					messageClient(%client,'',"\c0" @ %show @ "\c6 has Build Rights.");
					%any++;
				}
			}
			if ( !%any )
				messageClient(%client,'',"\c6No users currently have building rights.");
			else if ( %any > 5 )
				messageClient(%client,'',"\c3Page Up to see all users.");
		}
		else 
		{
			for( %i=0; %i < getWordCount($Pref::Server::BuildBlacklist); %i++ )
			{
				%banned = getWord($Pref::Server::BuildBlacklist, %i);
				%show = isObject(findClientByBL_ID(%banned)) ? findClientByBL_ID(%banned).name : "BL_ID " @ %banned;
				if ( %show !$= "" )
				{
					messageClient(%client,'',"\c0" @ %show @ "\c6 is banned from building.");
					%any++;
				}
			}
			if ( !%any )
				messageClient(%client,'',"\c6No users are currently banned from building.");
			else if ( %any > 5 )
				messageClient(%client,'',"\c3Page Up to see all users.");
		}
	
	// remove all bans
	case "clearRights":
		if ( $Pref::Server::BuildingBlacklist::WhitelistMode )
		{
			$Pref::Server::BuildWhitelist = "";
			messageClient(%client,'',"\c6All building rights have been cleared.");
		}
		else 
		{
			$Pref::Server::BuildBlacklist = "";
			messageClient(%client,'',"\c6All building bans have been cleared.");
		}
		
	// toggle which server mode
	case "toggleWhitelist":
		ToggleWhiteList();
		
	// invalid arguments 
	default:
		messageClient(%client,'',"\c0Error: Invalid command\n\c6List of commands:\n\c6 giveRights\n\c6 takeRights\n\c6 clearRights\n\c6 listRights\n\c6 toggleWhitelist");	
	}
}
