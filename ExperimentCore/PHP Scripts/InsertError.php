<?php
require_once('DbConection.php');
$db_connection = connect_to_db();

$clientId = !empty($_GET['clientId']) ? $_GET['clientId'] : "NULL";
$error = !empty($_GET['error']) ? $_GET['error'] : "NULL";
$sql = "INSERT INTO ".get_db_name().".errors ".
		"(ClientId, Error, TimeOfError)" .
		" VALUES ".
		"('$clientId', '$error', STR_TO_DATE('" . $_GET['date'] . "','%m-%d-%Y %H:%i:%s'))";	

if(mysqli_query($db_connection, $sql))
{
	echo(";ID=".mysqli_insert_id($db_connection).";");
}
else
{
	echo(";ERROR=". mysqli_error($db_connection).";");	
}

mysqli_close($db_connection);
?>