<?php

function get_db_name()
{
	return "dfpt";
}

/***************************
Connect to mysql DB, and returns the connection object.
***************************/
function connect_to_db()
{
	$server_name = '10.240.0.12';#'104.155.17.89';
	$username = 'sqluser';
	$password = 'Password1';
	$conn = mysqli_connect($server_name, $username, $password);

	if(!$conn)
	{
		echo(";ERROR=". mysqli_connect_error().";");	
		die(";ERROR=". mysqli_connect_error().";");
	}
	return $conn;
}

/***************************
Connect to mysql DB, and returns the connection object.
***************************/
function check_connection_to_db()
{
	$server_name = '10.240.0.12';#'104.155.17.89';
	$username = 'sqluser';
	$password = 'Password1';
	$conn = mysqli_connect($server_name, $username, $password);

	if(!$conn)
	{
		echo(";CONNECTION=CLOSED;");	
	}
	else
	{
		echo ";CONNECTION=OPEN;";
	}
	mysqli_close($conn);
}

?>