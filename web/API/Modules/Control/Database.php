<?php

    namespace API\Modules\Control;    
    class Database
    {
        // PDO Connection instance
        public $PDO;
        
        // Connection settings
        const desination = "localhost";
        const username = "root";
        const password = "";
        const database = "challenger";
        
        // Connection options
        protected $options = array(
            \PDO::MYSQL_ATTR_INIT_COMMAND => 'SET NAMES utf8',
        ); 
        
        function __construct()
        {
            $DSN = "mysql:host=".self::desination.";dbname=".self::database;
            
            try
            {
                $this->PDO = new \PDO($DSN, self::username, self::password, $this->options);
                return;
                
            }
            catch(\PDOException $message)
            {
                exit ("PDOException");
            }
        }
    }

?>