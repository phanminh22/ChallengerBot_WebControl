<?php
	global $PDO;
	require_once (__DIR__."/../Core.php");
	$Core = new Core();

	if ($_GET['clear'] == "true")
	{
		$Console = $PDO->prepare("TRUNCATE TABLE `console`");
		$Console->execute();
		exit;
	}

	// Last response time
	$ResponseTime = $PDO->query("SELECT response FROM `settings` WHERE label = 'CBot'");
	$Response = $ResponseTime->fetch(PDO::FETCH_ASSOC);

	// All console data
	$Console = $PDO->query("SELECT * FROM `console` WHERE `timestamp` >= ".$Response['response']." ORDER BY `timestamp` ASC");
	$ConsoleLog = $Console->fetchAll(PDO::FETCH_ASSOC);

	if(count($ConsoleLog) < 1)
	{
		printf("<tr> <td><font color = '#990033'>%s</font></td>  <td><font color = '#666633'>%s</font></td>	<td>%s</td><tr>", "00:00:00", "-", "-");
		exit;
	}


	foreach ($ConsoleLog as $data) {
		printf("<tr> <td><font color = '#990033'>%s</font></td>  <td><font color = '#666633'>%s</font></td>	<td>%s</td><tr>",
				date("H:i:s", $data['timestamp']),
					$data['player'],
						$data['content']);
	}

?>
