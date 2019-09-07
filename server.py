import requests
import json
import time
import threading
import socket
import random


body = {
    "msg_data_id": 2247485094,
    "begin": 0,
    "count": 50,
    "type": 0
}


def timestamp_datetime(value):
    format = '%Y-%m-%d %H:%M:%S'
    # value为传入的值为时间戳(整形)，如：1332888820
    value = time.localtime(value)
    ## 经过localtime转换后变成
    ## time.struct_time(tm_year=2012, tm_mon=3, tm_mday=28, tm_hour=6, tm_min=53, tm_sec=40, tm_wday=2, tm_yday=88, tm_isdst=0)
    # 最后再经过strftime函数转换为正常日期格式。
    dt = time.strftime(format, value)
    return dt

def get(url, datas=None):
    response = requests.get(url, params=datas)
    json = response.json()
    return json


def post(url, datas=None):
    response = requests.post(url, data=datas)
    json = response.json()
    return json

class Reader(threading.Thread):
    def __init__(self, client):
        threading.Thread.__init__(self)
        self.client = client

    def run(self):
        while True:
            data = self.client.recv(1024)
            if data:
                body["msg_data_id"] = int(bytes.decode(data, 'utf-8'))
                json0 = get(
                    'https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=xxxxxx&secret=xxxxxx')
                token = json0["access_token"]
                print(token)
                json1 = post('https://api.weixin.qq.com/cgi-bin/comment/list?access_token=' + token, json.dumps(body))
                count = json1['comment'][0]['user_comment_id'] // 50
                for a in range(0, count + 1):
                    body["begin"] = a * 50
                    json1 = post('https://api.weixin.qq.com/cgi-bin/comment/list?access_token=' + token, json.dumps(body))
                    for i in json1['comment']:
                        name = get(
                            'https://api.weixin.qq.com/cgi-bin/user/info?access_token=' + token + '&openid=' + i['openid'])
                        if 'remark' in name.keys():
                            if name['remark'] is not '':
                                self.client.sendall((str(i['user_comment_id']) +'[有备注]\n'+ name["remark"] + ':' + i['content'] + ' 时间：' + timestamp_datetime(
                                    i['create_time'])+'\n').encode('utf-8'))
                            else:
                                self.client.sendall((str(i['user_comment_id']) +'[无备注]\n'+ name["nickname"] + ':' + i['content'] + ' 时间：' + timestamp_datetime(
                                        i['create_time'])+'\n').encode('utf-8'))
                        else:
                            self.client.sendall((str(i['user_comment_id']) +'[游客]\n'+ name["openid"] + ':' + i['content'] + ' 时间：' + timestamp_datetime(
                                    i['create_time'])+'\n').encode('utf-8'))

                break
        print("close:", self.client.getpeername())
        self.client.close()


class Listener(threading.Thread):
    def __init__(self, port):
        threading.Thread.__init__(self)
        self.port = port
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        self.sock.bind(('', port))
        self.sock.listen(0)

    def run(self):
        print("listener started")
        while True:
            print('start another')
            client, cltadd = self.sock.accept()
            Reader(client).start()
            print("accept a connect:", cltadd)

if __name__ == '__main__':
    lst = Listener(3389)  # create a listen thread
    lst.start()  # then start
