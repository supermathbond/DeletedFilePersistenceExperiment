<?php
require_once('DbConection.php');
$db_connection = connect_to_db();

$cpu = !empty($_GET['Cpu']) ? $_GET['Cpu'] : "NULL";
$Ram = !empty($_GET['Ram']) ? $_GET['Ram'] : "NULL";
$FreeSpace = !empty($_GET['FreeSpace']) ? $_GET['FreeSpace'] : "NULL";
$DiskSize = !empty($_GET['DiskSize']) ? $_GET['DiskSize'] : "NULL";
$OS = !empty($_GET['OS']) ? $_GET['OS'] : "NULL";
$ServicePack = !empty($_GET['ServicePack']) ? $_GET['ServicePack'] : "NULL";
$is64Bit = !empty($_GET['is64Bit']) ? $_GET['is64Bit'] : "NULL";
$HostName = !empty($_GET['HostName']) ? $_GET['HostName'] : "NULL";
$UserName = !empty($_GET['UserName']) ? $_GET['UserName'] : "NULL";	
$Version = !empty($_GET['Version']) ? $_GET['Version'] : "NULL";	
$sql = "INSERT INTO ".get_db_name().".ClientsDetails ".
		"(Cpu, Ram, FreeSpace, DiskSize, OS, ServicePack, is64Bit, HostName, UserName, isFromCloud, Version)" .
		" VALUES ".
		"('$cpu', '$Ram', '$FreeSpace', '$DiskSize', '$OS', '$ServicePack', '$is64Bit', '$HostName', '$UserName', 0, '$Version')";	

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