<?php

	$secret = "test";
	$cachedir = "sessions";
	
	// end of configuration

	if(isset($_POST["data"]) && $_POST["key"] == $secret) {
	
		if($_POST["compression"] == "true") {
			$data = gzinflate(base64_decode($_POST["data"]));
			if($data == false) {
				echo "Unable to inflate!";
				die();
			}
		}
		else
			$data = urldecode(stripslashes($_POST["data"]));

		$rebuild = false;
				
		if((int)$_POST["ssid"] > 0) {
			$filename = $cachedir ."/". $_POST["ssid"] .".json";
			if(!is_file($filename))
				$rebuild = true;
			$fp = fopen($filename, 'w+');
			fwrite($fp, $data, strlen($data));
			fclose($fp);
		}
		else {
			echo "Session ID error!";
		}
		
		if($rebuild)
			set_current($_POST["ssid"]);
	}

	else if ($_GET["phpinfo"])
		phpinfo();
	else if ($_POST["key"] != $secret)
		echo "Key error!";
	else
		echo "General error!";

	function set_current($ssid) {
		global $cachedir;		
		$data = $ssid;
		$fp = fopen($cachedir ."/current.txt", "w+");
		fwrite($fp, $data, strlen($data));
		fclose($fp);
	}	

?>
