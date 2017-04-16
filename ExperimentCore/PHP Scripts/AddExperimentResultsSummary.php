<?php
require_once('DbConection.php');

$db_connection = connect_to_db();

$sql = "INSERT INTO ".get_db_name().".experimentresultssummary (MissionId, DateTime, HowManyAlive) VALUES ".
			"(". $_GET['MissionId'].",STR_TO_DATE('" . $_GET['DateTime'] . "','%m-%d-%Y %H:%i:%s'),".$_GET['HowManyAlive'].")";
if(mysqli_query($db_connection, $sql))
{
	echo("OP=SUCCESS;");
}
else
{
	echo("ERROR=". mysqli_error($db_connection).";");	
}


mysqli_close($db_connection);
?>