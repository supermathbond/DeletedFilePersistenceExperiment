<?php
require_once('DbConection.php');

function IsNullOrEmptyString($question){
    return (!isset($question) || trim($question)==='');
}

$db_connection = connect_to_db();
$values = explode(";",$_POST['parsedData']);
foreach ($values as $value) 
{
    if(!IsNullOrEmptyString($value))
	{
		$data = explode(",",$value);
		$sql = "INSERT INTO ".get_db_name().".ExperimentResults (MissionId, DateTime, IsAllocated, LCN, AlivePercent) VALUES ".
			"(". $_POST['MissionId'].",STR_TO_DATE('" . $_POST['DateTime'] . "','%m-%d-%Y %H:%i:%s'),".$data[0].",".$data[1].",".$data[2].")";
		if(mysqli_query($db_connection, $sql))
		{
			echo("OP=SUCCESS;");
		}
		else
		{
			echo("ERROR=". mysqli_error($db_connection).";");	
		}
	}
}

mysqli_close($db_connection);
?>