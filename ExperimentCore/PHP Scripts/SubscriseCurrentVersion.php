<?php
require_once('DbConection.php');
$db_connection = connect_to_db();

$Id = !empty($_GET['Id']) ? $_GET['Id'] : "NULL";
$Version = !empty($_GET['Version']) ? $_GET['Version'] : "NULL";
$CurrentFolder = !empty($_GET['CurrentFolder']) ? $_GET['CurrentFolder'] : "NULL";
$sql = "UPDATE ".get_db_name().
		".ClientsDetails SET Version='$Version', CurrentFolder= '$CurrentFolder' WHERE Id=$Id";

if(!mysqli_query($db_connection, $sql))
{
	echo(";ERROR=". mysqli_error($db_connection).";");	
}

mysqli_close($db_connection);


?>