<?php
require_once('DbConection.php');
if(!empty($_GET['Id']))
{
	$db_connection = connect_to_db();
	$sql = "INSERT INTO ".get_db_name().".KeepAlive ".
			"(ClientId, KeepAliveDate, statusId) VALUES("
			.$_GET['Id'].", STR_TO_DATE('" . $_GET['Date'] . "','%m-%d-%Y %H:%i:%s'),".$_GET['StatusId'].") ".
			"ON DUPLICATE KEY UPDATE ".
			"KeepAliveDate=STR_TO_DATE('" . $_GET['Date'] . "','%m-%d-%Y %H:%i:%s'), statusId=".$_GET['StatusId'];

	if(mysqli_query($db_connection, $sql))
	{
		echo(";RESULT=SUCCESS;");
	}
	else
	{
		echo(";ERROR=". mysqli_error($db_connection).";");	
	}
	mysqli_close($db_connection);
}
else
{
	echo(";ERROR=NO ID was given;");
}

?>