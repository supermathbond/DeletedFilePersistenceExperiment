<?php
require_once('DbConection.php');
$db_connection = connect_to_db();

$sql = "SELECT ID, CurrentFolder FROM ".get_db_name().".clientsdetails";

$result = mysqli_query($db_connection, $sql);
if (mysqli_num_rows($result) > 0) 
{
	while($row = mysqli_fetch_assoc($result))
	{
		echo $row["ID"]. ";" . $row["CurrentFolder"]."<br />";
	}
}

mysqli_close($db_connection);

?>