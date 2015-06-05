<?php 
	
	namespace API;
	class Autoloader 
	{
		private $prefix;
		private $prefixLength;
		private $directory;
        
		public function __construct($baseDirectory = __DIR__)
		{
			$this->directory = $baseDirectory;
			$this->prefix = __NAMESPACE__ . '\\';
			$this->prefixLength = strlen($this->prefix);
		}

		public static function register()
		{
            spl_autoload_extensions('.php');
			spl_autoload_register(array(new self, 'autoload'), true);
		}

		public function autoload($className)
	    {
	        if (0 === strpos($className, $this->prefix) && !class_exists($className)) 
	        {
	            $parts = explode('\\', substr($className, $this->prefixLength));
	            $filepath = $this->directory.DIRECTORY_SEPARATOR.implode(DIRECTORY_SEPARATOR, $parts).'.php';
	            if (is_file($filepath)) {
	                require($filepath);
	            }
	        }
	    }
	}
    
?>