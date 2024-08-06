<?php
set_time_limit(0);
ob_implicit_flush();

$address = '0.0.0.0';
$port = 25;

// Sunucu soketi oluşturma
$sock = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
if ($sock === false) {
    echo "Socket oluşturulamadı: " . socket_strerror(socket_last_error()) . "\n";
    exit;
}

// Sunucu soketini belirli bir adrese ve porta bağlama
if (socket_bind($sock, $address, $port) === false) {
    echo "Socket bağlanamadı: " . socket_strerror(socket_last_error($sock)) . "\n";
    exit;
}

// Gelen bağlantıları dinleme
if (socket_listen($sock, 5) === false) {
    echo "Socket dinlenemedi: " . socket_strerror(socket_last_error($sock)) . "\n";
    exit;
}

echo "SMTP sunucusu başlatıldı...\n";

do {
    // Gelen bağlantıları kabul etme
    $client = socket_accept($sock);
    if ($client === false) {
        echo "Socket kabul edilmedi: " . socket_strerror(socket_last_error($sock)) . "\n";
        break;
    }
    echo "Yeni bir istemci bağlandı...\n";

    // Sunucu bağlantı mesajı
    $welcomeMessage = "220 localhost Simple SMTP Server\r\n";
    socket_write($client, $welcomeMessage, strlen($welcomeMessage));

    // İstemciden gelen verileri okuma
    while ($request = socket_read($client, 1024)) {
        echo "İstemciden gelen: " . $request . "\n";
        if(preg_match("/Subject:\sAuthCode:\s(\d+)/",$request,$matches)){
                if(isset($matches[1])){
                        echo $matches[1].PHP_EOL;
                }
        }
        // SMTP komutlarına yanıt verme
        if (stripos($request, "HELO") === 0 || stripos($request, "EHLO") === 0) {
            $response = "250 Hello\r\n";
            socket_write($client, $response, strlen($response));
        } elseif (stripos($request, "MAIL FROM:") === 0) {
            $response = "250 OK\r\n";
            socket_write($client, $response, strlen($response));
        } elseif (stripos($request, "RCPT TO:") === 0) {
                preg_match('/RCPT TO:<(\d+)\@/', $request, $matches);
                if (isset($matches[1])) {
                        $telefonNumarasi = $matches[1];
                        echo "Telefon numarası: " . $telefonNumarasi . "\n";
                } else {
                        echo "Telefon numarası bulunamadı.\n";
                }
            $response = "250 OK\r\n";
            socket_write($client, $response, strlen($response));
        } elseif (stripos($request, "DATA") === 0) {
                var_export($request);
            $response = "354 End data with <CR><LF>.<CR><LF>\r\n";
            socket_write($client, $response, strlen($response));
        } elseif (trim($request) == ".") {
            $response = "250 OK: Message accepted for delivery\r\n";
            socket_write($client, $response, strlen($response));
        } elseif (stripos($request, "QUIT") === 0) {
            $response = "221 Bye\r\n";
            socket_write($client, $response, strlen($response));
            break;
        } else {
            $response = "500 Command not recognized\r\n";
            socket_write($client, $response, strlen($response));
        }
    }

    // İstemci bağlantısını kapatma
    socket_close($client);
    echo "İstemci bağlantısı kapatıldı.\n";

} while (true);

// Sunucu soketini kapatma
socket_close($sock);
?>
