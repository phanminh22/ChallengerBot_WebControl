<?php

	class Core
	{
		public function Core()
		{
			require_once (dirname(__DIR__)."/API/Autoloader.php");
			API\Autoloader::register();
			Core::Database();
		}

		public static function Database()
		{
			global $PDO;
            $Database = new API\Modules\Control\Database();
            $PDO = $Database->PDO;
		}
	}

?>