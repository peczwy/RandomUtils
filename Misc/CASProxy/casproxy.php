<html>
	<head>
		<title>CASProxy</title>
	</head>
	<body>
		<?php
			$key = @TO_REPLACE;
			$validDate = '2019-06-14 00:00:00';
			$now = date("Y-m-d H:i:s");
			if($_GET['key'] == $key && strcasecmp($now, $validDate) < 0){	
				$service = urlencode("http://www.mpenar.kia.prz.edu.pl/casproxy.php?redirect=". $_GET['redirect'] ."&key=" . $key);
				$curl = curl_init();
				curl_setopt($curl, CURLOPT_CUSTOMREQUEST, "POST");
				curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);
				curl_setopt($curl, CURLOPT_URL, 'https://cas.prz.edu.pl/cas-server/serviceValidate?service='. $service .'&ticket=' . $_GET['ticket']);
				$result = curl_exec($curl);
				curl_close($curl);
				
				print_r("<div id=\"ticket\">");
				print_r(base64_encode($result));
				print_r("</div>");
			}else{
				print_r("<div id=\"ticket\">");
				print_r('Access Forbidden');
				print_r("</div>");
			}
		?> 
		
		<script type="text/javascript">
			var url_string = window.location.href;
			var url = new URL(url_string);
			var redirectUrl = url.searchParams.get("redirect");
			window.location.replace(redirectUrl + "?response=" + document.getElementById("ticket").textContent);
		</script>
	</body>
</html>