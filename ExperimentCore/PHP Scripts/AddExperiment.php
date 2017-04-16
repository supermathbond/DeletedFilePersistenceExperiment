<?php
require_once('DbConection.php');
$db_connection = connect_to_db();

$sql = "INSERT INTO ".get_db_name().".ExperimentDetails (DateOfAttack, TimeGap, NumOfClustersToCreate, ClusterData) VALUES (STR_TO_DATE('" . $_POST['DateOfAttack'] . "','%m-%d-%Y %H:%i:%s'),".$_POST['TimeGap'].",".$_POST['NumOfClustersToCreate'].",'".$_POST['ClusterData']."')";

if(mysqli_query($db_connection, $sql))
{
	echo(";OP=SUCCESS;");

	$id = mysqli_insert_id($db_connection);

	$sql2 = "INSERT INTO ".get_db_name().".Missions (ClientID, isMissionFinished, MissionTypeID, DateTime, TempFolder, ExpID) VALUES (".
		$_POST['ClientID'].",0,1,NOW(),'".$_POST['TempFolder']."'," . strval($id) .")";

	if(mysqli_query($db_connection, $sql2))
	{
		echo(";OP=SUCCESS;");
	}
	else
	{
		echo(";ERROR=". mysqli_error($db_connection).";");	
	}

}
else
{
	echo(";ERROR=". mysqli_error($db_connection).";");	
}

mysqli_close($db_connection);

?>