<?php
require_once('DbConection.php');
$db_connection = connect_to_db();

$sql = "SELECT m.ID as id, m.MissionTypeID as typeId, m.DateTime as missionTimeCreation, m.TempFolder as TempFolder, m.MoreDetails as MoreDetails, ".
		"e.DateOfAttack as DateOfAttack, e.TimeGap as TimeGap, e.ClusterData as ClusterData, e.NumOfClustersToCreate as NumOfClustersToCreate ".
		"FROM ".get_db_name().".Missions m LEFT OUTER JOIN ".get_db_name().".ExperimentDetails e on(m.ExpID = e.ExpId) ".
		"where (m.ClientID = " . $_GET['clientId'] . " or m.ClientID is null) and isMissionFinished=0 ";
if(!empty($_GET['from']))
	$sql = $sql . " and m.DateTime >= STR_TO_DATE('" . $_GET['from'] . "','%m-%d-%Y %H:%i:%s')";
$sql = $sql . " ORDER BY missionTimeCreation";
$result = mysqli_query($db_connection, $sql);
if (mysqli_num_rows($result) > 0) 
{
    // output data of each row
    while($row = mysqli_fetch_assoc($result)) {
        echo $row["id"]. "," . $row["typeId"]. "," . $row["missionTimeCreation"]. ",".$row["TempFolder"]. ",".str_replace(",", "", $row["MoreDetails"]). 
		",".$row["DateOfAttack"]. ",".$row["TimeGap"].",".$row["ClusterData"].",".$row["NumOfClustersToCreate"].",". "<br />";
    }
}

mysqli_close($db_connection);



?>