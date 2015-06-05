<?php
    global $PDO;
    require_once (__DIR__."/../Core.php");
    $Core = new Core();
    $PDO->prepare("SET sql_mode='NO_UNSIGNED_SUBTRACTION';")->execute();

    $Account = $_POST['account'];
    $Password = $_POST['password'];
    $MaxLevel = (int)$_POST['maxlevel'];
    $AutoBoost = (int)$_POST['boost'];

    $Insertion = $PDO->prepare("INSERT IGNORE INTO `accounts` SET password = :password, maxlevel = :maxlevel, autoboost = :autoboost, account = :account");
    $Insertion->bindParam(":account", $Account, PDO::PARAM_STR);
    $Insertion->bindParam(":password", $Password, PDO::PARAM_STR);
    $Insertion->bindParam(":maxlevel", $MaxLevel, PDO::PARAM_INT);
    $Insertion->bindParam(":autoboost", $AutoBoost, PDO::PARAM_INT);
    $Insertion->execute() or die (var_dump($Insertion->errorInfo()));
    header("Location: ../../index.php");
?>
