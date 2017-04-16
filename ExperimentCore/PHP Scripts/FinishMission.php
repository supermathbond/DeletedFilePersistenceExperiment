<?php
require_once('DbConection.php');
$db_connection = connect_to_db();

if(empty($_GET['MissionId']))
{
	echo (";ERROR=No mission ID;");	
}
else if(empty($_GET['ClientId']))
{
	echo (";ERROR=No Client ID;");	
}

$sql = "UPDATE ".get_db_name().".Missions SET isMissionFinished = 1 ".
		"WHERE ID=".$_GET['MissionId']." AND ClientID=".$_GET['ClientId'];
		
if(mysqli_query($db_connection, $sql))
{
	echo(";OP=SUCCESS;");
}
else
{
	echo(";ERROR=". mysqli_error($db_connection).";");	
}

mysqli_close($db_connection);

?>