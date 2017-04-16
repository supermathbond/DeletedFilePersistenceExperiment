<?php
require_once('DbConection.php');
$db_connection = connect_to_db();
if(empty($_GET['clientId']) || $_GET['clientId'] == "")
	die("");

$sql = "SELECT FreeText, CurrentFolder FROM ".get_db_name().".clientsdetails WHERE ID=".$_GET['clientId'];
$result = mysqli_query($db_connection, $sql);
if (mysqli_num_rows($result) > 0) 
{
    // output data of each row
    while($row = mysqli_fetch_assoc($result)) {
        echo $row["CurrentFolder"]. ";".$row["FreeText"];
		break;
    }
}

mysqli_close($db_connection);


?>