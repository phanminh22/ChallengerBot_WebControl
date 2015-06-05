<?php

    ob_start();
    global $PDO;
    require_once ("API/Core.php");
    $Core = new Core();
    
?>

<!DOCTYPE html>
<html lang="en">
<head>
	<meta http-equiv="content-type" content="text/html; charset=UTF-8">
	<meta charset="utf-8">
	<title>Bootstrap 3 Admin</title>
	<meta name="generator" content="Bootply" />
	<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1">
	<link href="css/bootstrap.min.css" rel="stylesheet">
		<!--[if lt IE 9]>
			<script src="//html5shim.googlecode.com/svn/trunk/html5.js"></script>
			<![endif]-->
			<link href="css/styles.css" rel="stylesheet">
			<script src="//ajax.googleapis.com/ajax/libs/jquery/2.0.2/jquery.min.js"></script>
			<script src="js/bootstrap.min.js"></script>
			<script src="js/scripts.js"></script>
		</head>

		<body>
			<!-- header -->
			<div id="top-nav" class="navbar navbar-inverse navbar-static-top">
				<div class="container-fluid">
					<div class="navbar-header">
						<button type="button" class="navbar-toggle" data-toggle="collapse" data-target=".navbar-collapse">
							<span class="icon-bar"></span>
							<span class="icon-bar"></span>
							<span class="icon-bar"></span>
						</button>
						<a class="navbar-brand" href="#">Dashboard</a>
					</div>
				</div>
				<!-- /container -->
			</div>
			<!-- /Header -->

			<!-- Main -->
			<div class="container-fluid">

				<div class="btn-group btn-group-justified">
					<a title="Add Account" data-toggle="modal" href="#addAccountModal" class="btn btn-primary col-sm-3">
						<span class="glyphicon glyphicon-plus"></span>
						<br> Add Account
					</a>
					<a title="Settings" data-toggle="modal" href="#Settings" class="btn btn-primary col-sm-3">
						<span class="glyphicon glyphicon-cog"></span>
						<br> Settings
					</a>
				</div>

				<hr>

				<div class="row">
					<!-- center left-->
					<div class="col-md-6">
						<div class="panel panel-default">
							<div class="panel-heading">
								<h4>Accounts </h4></div>
								<div class="panel-body">
									<div class="table-responsive" style="max-height: 300px; overflow: auto;">
										<table class="table table-striped" style = "margin-bottom: 0px;" id = "accountbox">
											<thead>
												<tr>
													<th>Account</th>
													<th>Level</th>
													<th>Influence Points</th>
													<th>Actions</th>
												</tr>
											</thead>
											<tbody>
											</tbody>
										</table>
									</div>
								</div>
							</div>
						</div>
						<!--/col-->
						<div class="col-md-6">
							<!--tabs-->
							<div class="panel">
								<ul class="nav nav-tabs" id="myTab">
									<li class="active"><a href="#console" data-toggle="tab">Console</a></li>
								</ul>
								<div class="tab-content">
									<div class="tab-pane active well" id="console">
										<div class="table-responsive" style="max-height: 300px; overflow: auto;">
											<table class="table table-hover" style = "margin-bottom: 0px;" id = "msgbox">
												<thead>
													<tr>
														<th>Time</th>
														<th>Player</th>
														<th>Message</th>
													</tr>
												</thead>
												<tbody>
												</tbody>
											</table>
										</div>
									</div>
								</div>

							</div>
							<!--/tabs-->
							


							

						</div>
						<!--/col-span-6-->
					</div>
					<!--/row-->
				</div>
				<!--/col-span-9-->
				
				<!-- /Main -->

				<footer class="text-center">This Bootstrap 3 dashboard layout is compliments of <a href="http://www.bootply.com/85850"><strong>Bootply.com</strong></a></footer>

				<?php

				if (isset($_GET['edit']))
				{
					$ID = (int)$_GET['edit'];
					$WhyYouDoThis = $PDO->query("SELECT COUNT(*) FROM `accounts` WHERE id = '".$ID."'")->fetchColumn();
					if ($WhyYouDoThis < 1)
					{
						header("Location: index.php");
						exit;
					}

					$Before = $PDO->query("SELECT * FROM `accounts` WHERE id = '".$ID."'");
					$Account = $Before->fetch(PDO::FETCH_ASSOC);

					?>

					<script type="text/javascript">
						$(window).load(function(){
							$('#Editing').modal('show');
						});
					</script>

					<div class="modal fade" id="Editing">
						<div class="modal-dialog">
							<div class="modal-content">
								<div class="modal-header">
									<button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
									<h4 class="modal-title">Edit Account</h4>
								</div>
								<div class="modal-body">
									<form role="form" method="post" action="API/Control/EditAccount.php">
										<div class="form-group">
											<input name="lastnameused" type="hidden" class="form-control" value="<?php echo $Account['account']; ?>">
										</div>
										<div class="form-group">
											<label for="account">Account:</label>
											<input name="account" class="form-control" id="account" placeholder="Enter account name" value="<?php echo $Account['account']; ?>">
										</div>
										<div class="form-group">
											<label for="accountpassword">Password:</label>
											<input name="password" type="password" class="form-control" id="accountpassword" placeholder="Enter account password" value="<?php echo $Account['password']; ?>">
										</div>
										<div class="form-group">
											<label for="maxlevel">Max level [1-30]:</label>
											<input name="maxlevel" type="number" min = "1" max = "30" class="form-control" id="maxlevel" placeholder="Max level" value="<?php echo $Account['maxlevel']; ?>">
										</div>
										<div class="checkbox">
											<label>
												<input name="boost" value="0" type="checkbox" checked="checked" style="display:none;">
												<input name="boost" value="1" type="checkbox"> Buy XP boost @ level 3
											</label>
										</div>
										
										<div class="modal-footer">
											<a href="index.php" data-dismiss="modal" class="btn">Close</a>
											<button type="submit" class="btn btn-primary">Save changes</button>
										</div>
									</form>
								</div>
							</div>
							<!-- /.modal-content -->
						</div>
						<!-- /.modal-dalog -->
					</div>

					<?php
				}

				if (isset($_GET['remove']))
				{

					?>

					<script type="text/javascript">
						$(window).load(function(){
							$('#Removed').modal('show');
						});
					</script>

					<div class="modal fade" id="Removed">
						<div class="modal-dialog">
							<div class="modal-content">
								<div class="modal-header">
									<button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
									<h4 class="modal-title">Removed!</h4>
								</div>
								<div class="modal-body">
									<p>This account has been removed successfully!</p>
								</div>
								<div class="modal-footer">
									<a href="#" data-dismiss="modal" class="btn">Close</a>
								</div>
							</div>
							<!-- /.modal-content -->
						</div>
						<!-- /.modal-dalog -->
					</div>

					<?php

					$ID = (int)$_GET['remove'];
					$WhyYouDoThis = $PDO->query("SELECT COUNT(*) FROM `accounts` WHERE id = '".$ID."'")->fetchColumn();
					if ($WhyYouDoThis < 1)
					{
						header("Location: index.php");
						exit;
					}


					$Q = $PDO->prepare("DELETE FROM `accounts` WHERE id = '".$ID."'");
					$Q->execute();
				}

				?>

				<div class="modal fade" id="Settings">
                    <?php
                        
                            $NoConfig = $PDO->query("SELECT COUNT(*) FROM `settings` WHERE label = 'CBot'")->fetchColumn();
					        if ($NoConfig < 1)
					        {
                                $PDO->prepare("INSERT INTO `settings` SET gamepath = 'C:\Path\To\Your\Game\'");
					        }

					        $Configs = $PDO->query("SELECT * FROM `settings` WHERE label = 'CBot'");
					        $Settings = $Configs->fetch(PDO::FETCH_ASSOC);
                    
                    ?>
					<div class="modal-dialog">
						<div class="modal-content">
							<div class="modal-header">
								<button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
								<h4 class="modal-title">Settings</h4>
							</div>
							<div class="modal-body">
								<form role="form" method="post" action="API/Control/Settings.php">
									<div class="form-group">
										<label for="gamepath">Game path (path to lol.launcher.exe):</label>
										<input value="<?php echo $Settings['gamepath']; ?>" name="gamepath" class="form-control" id="gamepath" placeholder="C:\Sumwer\">
									</div>
									<div class="form-group">
										<label for="platform">Platform: </label>
										<select name="platform" class="form-control" id="platform">
                                        <?php
                                            
                                        $SelectionString = $Settings['platform']."Selected";
                                        $$SelectionString = "selected";
                                        
											echo "<option value = 'EUW' ".$EUWSelected."> EUW </option>";
											echo "<option value = 'EUNE' ".$EUNESelected."> EUNE  </option>";
											echo "<option value = 'NA' ".$NASelected."> NA  </option>";
											echo "<option value = 'KR' ".$KRSelected."> KR  </option>";
											echo "<option value = 'BR' ".$BRSelected."> BR  </option>";
											echo "<option value = 'LA1' ".$LA1Selected."> LA1  </option>";
											echo "<option value = 'LA2' ".$LA2Selected."> LA2  </option>";
											echo "<option value = 'SG' ".$SGSelected."> SG  </option>";
											echo "<option value = 'MY' ".$MYSelected."> MY  </option>";
											echo "<option value = 'SGMY' ".$SGMYSelected."> SGMY  </option>";
											echo "<option value = 'TH' ".$THSelected."> TH  </option>";
											echo "<option value = 'PH' ".$PHSelected."> PH  </option>";
											echo "<option value = 'VN' ".$VNSelected."> VN  </option>";
											echo "<option value = 'OCE' ".$OCESelected."> OCE  </option>";
											echo "<option value = 'CS' ".$CSSelected."> CS  </option>";
                                            
                                        ?>
										</select>
									</div>
									<div class="form-group">
										<label for="game">Select game:</label>
										<select name="game" class="form-control" id="game">
											<option value = "25"> Dominion COOP </option>
											<option value = "32"> SummonersRift COOP  </option>
											<option value = "52"> TwistedThreeLine COOP  </option>
											<option value = "65"> Howling Abyss </option>
										</select>
									</div>

									<div class="form-group">
										<label for="difficulty">Select COOP difficulty:</label>
										<select name="difficulty" class="form-control" id="difficulty">
											<option value = "-"> ------- </option>
											<option value = "EASY"> Easy  </option>
											<option value = "MEDIUM"> Medium  </option>
										</select>
									</div>
									<div class="form-group">
										<label for="players">Max players [1-5]:</label>
										<input value="<?php echo $Settings['players']; ?>" name="players" type="number" min = "1" max = "5" class="form-control" id="players">
									</div>
									<div class="modal-footer">
										<a href="#" data-dismiss="modal" class="btn">Close</a>
										<button type="submit" class="btn btn-primary">Save changes</button>
									</div>
								</form>
							</div>

						</div>
						<!-- /.modal-content -->
					</div>
					<!-- /.modal-dalog -->
				</div>

				<div class="modal fade" id="addWidgetModal">
					<div class="modal-dialog">
						<div class="modal-content">
							<div class="modal-header">
								<button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
								<h4 class="modal-title">Add Widget</h4>
							</div>
							<div class="modal-body">
								<p>Add a widget stuff here..</p>
							</div>
							<div class="modal-footer">
								<a href="#" data-dismiss="modal" class="btn">Close</a>
								<button type="submit" class="btn btn-primary">Save changes</button>
							</div>
						</div>
						<!-- /.modal-content -->
					</div>
					<!-- /.modal-dalog -->
				</div>

				<div class="modal fade" id="addAccountModal">
					<div class="modal-dialog">
						<div class="modal-content">
							<div class="modal-header">
								<button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>
								<h4 class="modal-title">Add Account</h4>
							</div>
							<div class="modal-body">
								<form role="form" method="post" action="API/Control/AddAccount.php">
									<div class="form-group">
										<label for="account">Account:</label>
										<input name="account" class="form-control" id="account" placeholder="Enter account name">
									</div>
									<div class="form-group">
										<label for="accountpassword">Password:</label>
										<input name="password" type="password" class="form-control" id="accountpassword" placeholder="Enter account password">
									</div>
									<div class="form-group">
										<label for="maxlevel">Max level [1-30]:</label>
										<input name="maxlevel" type="number" min = "1" max = "30" class="form-control" id="maxlevel">
									</div>
									<div class="checkbox">
										<label>
											<input name="boost" value="0" type="checkbox" checked="checked" style="display:none;">
											<input name="boost" value="1" type="checkbox"> Buy XP boost @ level 3
										</label>
									</div>
									
									<div class="modal-footer">
										<a href="#" data-dismiss="modal" class="btn">Close</a>
										<button type="submit" class="btn btn-primary">Add account</button>
									</div>
								</form>
							</div>
						</div>
						<!-- /.modal-content -->
					</div>
					<!-- /.modal-dalog -->
				</div>


				<!-- /.modal -->
				<!-- script references -->
				<script>

					function getMessages()
					{
						$.ajax(
						{
							url: "API/Result/Console.php",
							dataType: "html",
							success: function(html)
							{
								if (html.length > 1)
								{
									$('#msgbox tbody').html(html);
								}
							}
						})
					}

					function getAccounts()
					{
						$.ajax(
						{
							url: "API/Result/Accounts.php",
							dataType: "html",
							success: function(html)
							{
								if (html.length > 1)
								{
									$('#accountbox tbody').html(html);
								}
							}
						})
					}

					getAccounts();
					getMessages();
					setInterval(getAccounts, 985);
					setInterval(getMessages, 985);
				</script>
			</body>
			</html>
