<?php
	global $PDO;
	require_once (__DIR__."/../Core.php");
	$Core = new Core();

	// Last response time
	$ResponseTime = $PDO->query("SELECT response FROM `settings` WHERE label = 'CBot'");
	$Response = $ResponseTime->fetch(PDO::FETCH_ASSOC);

	// All console data
	$Console = $PDO->query("SELECT * FROM `accounts` ORDER BY `level` DESC");
	$ConsoleLog = $Console->fetchAll(PDO::FETCH_ASSOC);

	if(count($ConsoleLog) < 1)
	{
		printf("<tr> <td><font color = '#990033'>%s</font></td>  <td><font color = '#666633'>%s</font></td>	<td>%s</td></tr>", "-", "-", "-");
		exit;
	}

    //  glyphicon glyphicon-pencil
	foreach ($ConsoleLog as $data) {
        $Actions = "
        <a data-toggle = \"modal\" href = \"?remove=".$data['id']."\"><span class = \"glyphicon glyphicon-remove\">  </span> </a>
        <a data-toggle = \"modal\" href = \"?edit=".$data['id']."\"><span class = \"glyphicon glyphicon-pencil\">  </span> </a>

        ";
		printf(" <tr> <td><font color = '#990033'>%s</font></td>
        <td><font color = '#666633'>%s</font></td>
                <td>%s</td>
                    <td> %s </td>
                </tr>",
				$data['account'],
					$data['level'],
						$data['money'],
                            $Actions);
	}

?>
