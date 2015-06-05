<?php

    global $PDO;
    require_once (__DIR__."/../Core.php");
    $Core = new Core();
    $PDO->prepare("SET sql_mode='NO_UNSIGNED_SUBTRACTION';")->execute();
    
    $GamePath = $_POST['gamepath'];
    $Platform = $_POST['platform'];
    $Players = $_POST['players'];
    $Game = $_POST['game'];
    $Difficulty = $_POST['difficulty'];
    
    $WotAtDeEnd = substr($GamePath, -1, 1);
    if ($WotAtDeEnd != "\\")
        $GamePath = $GamePath."\\";
    
    if ($Players > 5)
        $Players = 5;
    
    $availableDiff = array("-", "EASY", "MEDIUM");
    $availableQueue = array(25, 52, 33, 32, 65);
    $ProbablyWorkingServers = array("EUW", "EUNE", "NA", "KR", "BR", "LA1", "LA2", "SG", "MY", "SGMY", "TH", "PH", "VN", "OCE", "CS");
    if (!in_array($Difficulty, $availableDiff) || !in_array($Game, $availableQueue) || !in_array($Platform, $ProbablyWorkingServers))
        exit;

    $HowHard = $Difficulty;
    $Queue = $Game;
    if ($Game == 32 && $Difficulty == "MEDIUM")
        $Queue = 33;
  
    
    $Settings = $PDO->prepare("INSERT INTO `settings` SET label = 'CBot', gamepath = :gamepath, platform = :platform, players = :players, queue = :queue, difficulty = :difficulty ON DUPLICATE KEY UPDATE gamepath = :updgamepath, platform = :updplarform, players = :updplayers, queue = :updqueue, difficulty = :upddifficulty ");
    $Settings->bindParam(":gamepath", $GamePath, PDO::PARAM_STR);
    $Settings->bindParam(":platform", $Platform, PDO::PARAM_STR);
    $Settings->bindParam(":players", $Players, PDO::PARAM_STR);
    $Settings->bindParam(":queue", $Queue, PDO::PARAM_INT);
    $Settings->bindParam(":difficulty", $Difficulty, PDO::PARAM_STR);
    $Settings->bindParam(":updgamepath", $GamePath, PDO::PARAM_STR);
    $Settings->bindParam(":updplarform", $Platform, PDO::PARAM_STR);
    $Settings->bindParam(":updplayers", $Players, PDO::PARAM_STR);
    $Settings->bindParam(":updqueue", $Queue, PDO::PARAM_INT);
    $Settings->bindParam(":upddifficulty", $Difficulty, PDO::PARAM_STR);
    $Settings->execute() or die (var_dump($Settings->errorInfo()));
    header("Location: ../../index.php");
?>